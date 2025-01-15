namespace Facticus.UI
{
    public class UiStateOpenInfo
    {
        public bool CloseOther;
        public bool KeepHistory;

        public static UiStateOpenInfo Default => new UiStateOpenInfo()
        {
            CloseOther = false,
            KeepHistory = false,
        };
        
        /// <summary>
        /// Close others
        /// </summary>
        public static UiStateOpenInfo CloseOtherStates => new UiStateOpenInfo()
        {
            CloseOther = true,
            KeepHistory = false,
        };
        
        /// <summary>
        /// Keep history
        /// </summary>
        public static UiStateOpenInfo Keep => new UiStateOpenInfo()
        {
            CloseOther = true,
            KeepHistory = true,
        };
    }
}