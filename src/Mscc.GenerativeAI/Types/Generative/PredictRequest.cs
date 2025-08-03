#if NET472_OR_GREATER || NETSTANDARD2_0
using System.Collections.Generic;
#endif

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// Request message for PredictionService.Predict.
    /// </summary>
    public class PredictRequest
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