namespace NAPS2.Wia
{
    /// <summary>
    /// WIA_DPS_PAGE_SIZE/WIA_IPS_PAGE_SIZE constants
    /// Dimensions are defined as (WIDTH x HEIGHT) in 1/1000ths of an inch
    /// </summary>
    public enum WiaPageSize : int
    {
        /// <summary>
        /// 8267 x 11692 in thousands of an inch
        /// </summary>
        A4 = 0,

        /// <summary>
        /// 8500 x 11000 in thousands of an inch, also knonw as (US) Letter
        /// </summary>
        Letter =  1,

        /// <summary>
        /// (current extent settings)
        /// </summary>
        Custom = 2,

        /// <summary>
        /// 8500 x 14000 in thousands of an inch
        /// </summary>
        UsLegal = 3,

        /// <summary>
        /// 11000 x 17000 in thousands of an inch
        /// </summary>
        UsLedger = 4,

        /// <summary>
        /// 5500 x  8500 in thousands of an inch
        /// </summary>
        UsStatement = 5,

        /// <summary>
        /// 3543 x  2165 in thousands of an inch
        /// </summary>
        BusinessCard = 6
    }
}