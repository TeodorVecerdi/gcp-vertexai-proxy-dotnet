namespace Mscc.GenerativeAI
{
    /// <summary>
    /// Configuration options that change client network behavior when testing.
    /// </summary>
    public class DebugConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string ClientMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReplayId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReplaysDirectory { get; set; }
    }
}