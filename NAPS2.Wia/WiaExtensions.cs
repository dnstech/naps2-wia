using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Wia
{
    public static class WiaExtensions
    {
        public static string Id(this IWiaDeviceProps device)
        {
            return device.Properties[WiaPropertyId.DIP_DEV_ID].Value.ToString();
        }

        public static string Name(this IWiaDeviceProps device)
        {
            return device.Properties[WiaPropertyId.DIP_DEV_NAME].Value.ToString();
        }

        public static string Name(this WiaItem item)
        {
            return item.Properties[WiaPropertyId.IPA_ITEM_NAME].Value.ToString();
        }

        public static string FullName(this WiaItem item)
        {
            return item.Properties[WiaPropertyId.IPA_FULL_ITEM_NAME].Value.ToString();
        }

        public static bool SupportsFeeder(this WiaDevice device)
        {
            int capabilities = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.FEEDER) != 0;
        }

        public static bool SupportsFlatbed(this WiaDevice device)
        {
            int capabilities = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.FLATBED) != 0;
        }

        public static bool SupportsDuplex(this WiaDevice device)
        {
            int capabilities = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.DUPLEX) != 0;
        }

        public static bool FeederReady(this WiaDevice device)
        {
            int status = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_STATUS].Value;
            return (status & WiaPropertyValue.FEED_READY) != 0;
        }

        /// <summary>
        /// Enables scanning double sided pages, make sure you check device SupportsDuplex before calling this.
        /// </summary>
        /// <param name="item">The feeder sub item.</param>
        public static void EnableDuplex(this WiaItem item)
        {
            item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX);
        }

        public static void SetAutoDeskewEnabled(this WiaItem item, bool enabled)
        {
            item.SetProperty(WiaPropertyId.IPS_AUTO_DESKEW, enabled ? (int)WiaAutoDeskew.On : (int)WiaAutoDeskew.Off);
        }

        public static void SetAutoCrop(this WiaItem item, WiaAutoCrop autoCrop)
        {
            item.SetProperty(WiaPropertyId.IPS_AUTO_CROP, (int)autoCrop);
        }

        /// <summary>
        /// Specifies the number of pages, zero will auto feed all pages from the feeder.
        /// NOTE: To get both sides of a duplex scan for a single piece of paper you will have to set page count to 2.
        /// </summary>
        /// <param name="item">The wia sub item from the device.</param>
        /// <param name="pageCount">0 or number of pages.</param>
        public static void SetPageCount(this WiaItem item, int pageCount = 0) 
        {
            item.SetProperty(WiaPropertyId.IPS_PAGES, pageCount);
        }

        public static void SetPageSize(this WiaItem item, WiaPageSize pageSize)
        {
            item.SetProperty(WiaPropertyId.IPS_PAGE_SIZE, (int)pageSize);
        }

        ////public static void SetCustomPageSize(this WiaItem item, float widthInInches, float heightInInches)
        ////{
        ////    item.SetProperty(WiaPropertyId.IPS_PAGE_SIZE, (int)WiaPageSize.Custom);
        ////    item.SetProperty(WiaPropertyId.IPS_XEXTENT, (int)(widthInInches * 1000));
        ////    item.SetProperty(WiaPropertyId.IPS_YEXTENT, (int)(heightInInches * 1000));
        ////}

        public static void SetColour(this WiaItem item, WiaColour color)
        {
            item.SetProperty(WiaPropertyId.IPA_DATATYPE, (int)color);
        }

        public static int TrySetDpi(this WiaItem item, int dpi)
        {
            item.SetPropertyClosest(WiaPropertyId.IPS_XRES, ref dpi);
            item.SetPropertyClosest(WiaPropertyId.IPS_YRES, ref dpi);
            return dpi;
        }

        public static void SetProperty(this WiaItemBase item, int propId, int value)
        {
            var prop = item.Properties.GetOrNull(propId);
            if (prop != null)
            {
                prop.Value = value;
            }
        }

        public static void SetPropertyClosest(this WiaItemBase item, int propId, ref int value)
        {
            var prop = item.Properties.GetOrNull(propId);
            if (prop != null)
            {
                if (prop.Attributes.Flags.HasFlag(WiaPropertyFlags.List))
                {
                    int value2 = value;
                    var choice = prop.Attributes.Values.OfType<int>().OrderBy(x => Math.Abs(x - value2)).Cast<int?>().FirstOrDefault();
                    if (choice != null)
                    {
                        prop.Value = choice.Value;
                        value = choice.Value;
                    }
                }
                else
                {
                    // Not a list, try to set the property directly
                    prop.Value = value;
                }
            }
        }

        public static void SetPropertyRange(this WiaItemBase item, int propId, int value, int expectedMin, int expectedMax)
        {
            var prop = item.Properties.GetOrNull(propId);
            if (prop != null)
            {
                if (prop.Attributes.Flags.HasFlag(WiaPropertyFlags.Range))
                {
                    int expectedAbs = value - expectedMin;
                    int expectedRange = expectedMax - expectedMin;
                    int actualRange = prop.Attributes.Max - prop.Attributes.Min;
                    int actualValue = expectedAbs * actualRange / expectedRange + prop.Attributes.Min;
                    if (prop.Attributes.Step != 0)
                    {
                        actualValue -= actualValue % prop.Attributes.Step;
                    }

                    actualValue = Math.Min(actualValue, prop.Attributes.Max);
                    actualValue = Math.Max(actualValue, prop.Attributes.Min);
                    prop.Value = actualValue;
                }
                else
                {
                    // Not a range, try to set the property directly
                    prop.Value = value;
                }
            }
        }

        public static Dictionary<int, object> SerializeEditable(this WiaPropertyCollection props)
        {
            return props.Where(x => x.Type == WiaPropertyType.I4).ToDictionary(x => x.Id, x => x.Value);
        }

        public static Dictionary<int, object> Delta(this WiaPropertyCollection props, Dictionary<int, object> target)
        {
            var source = props.SerializeEditable();
            var delta = new Dictionary<int, object>();
            foreach (var kvp in target)
            {
                if (source.ContainsKey(kvp.Key) && !Equals(source[kvp.Key], kvp.Value))
                {
                    delta.Add(kvp.Key, kvp.Value);
                }
            }
            return delta;
        }

        public static void DeserializeEditable(this WiaPropertyCollection props, Dictionary<int, object> values)
        {
            foreach (var kvp in values)
            {
                var prop = props.GetOrNull(kvp.Key);
                if (prop != null)
                {
                    try
                    {
                        prop.Value = kvp.Value;
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (WiaException)
                    {
                    }
                }
            }
        }
    }
}
