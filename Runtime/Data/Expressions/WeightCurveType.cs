namespace USTL.FaceTracking
{
    internal enum WeightCurveType
    {
        /// <summary>
        ///     x:  0 ---- 1<br />
        ///     y:  0 ---- 1
        /// </summary>
        Linear,

        /// <summary>
        ///     x: -1 ---- 0 ---- 1<br />
        ///     y:  0 ---- 0 ---- 1
        /// </summary>
        PositiveSigned,

        /// <summary>
        ///     x: -1 ---- 0 ---- 1<br />
        ///     y:  1 ---- 0 ---- 0
        /// </summary>
        NegativeSigned,

        /// <summary>
        ///     x:  0 ---- 0.75 ---- 1<br />
        ///     y:  1 ---- 0 ---- 0
        /// </summary>
        EyelidClosed,

        /// <summary>
        ///     x:  0 ---- 0.75 ---- 1<br />
        ///     y:  0 ---- 0 ---- 1
        /// </summary>
        EyelidWide,
    }
}
