using System.Collections.Generic;

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// Request message for [PredictionService.PredictLongRunning].
    /// </summary>
    public class PredictLongRunningRequest
    {
        /// <summary>
        /// Required. The instances that are the input to the prediction call.
        /// </summary>
        public List<object> Instances { get; set; }
        /// <summary>
        /// Optional. The parameters that govern the prediction call.
        /// </summary>
        public List<object> Parameters { get; set; }
    }
}