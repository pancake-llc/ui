namespace Pancake.UI
{
    public enum EButtonClickType
    {
        /// <summary>
        /// only executed single click.
        /// </summary>
        OnlySingleClick = 0,

        /// <summary>
        /// only executed double click
        /// </summary>
        OnlyDoubleClick = 1,

        /// <summary>
        /// execute button onClick event after a period of time
        /// remove double click executed
        /// </summary>
        LongClick = 2,

        /// <summary>
        /// normal click type (single click + double click)
        /// single click will get executed before a double click (dual actions)
        /// </summary>
        Instant = 3,

        /// <summary>
        /// if it's a double click, the single click will not executed
        /// use this if you want to make sure single click not execute before a double click
        /// the downside is that there is a delay when executing the single click (the delay is the double click register interval)
        /// </summary>
        Delayed = 4
    }

    public enum EButtonMotion
    {
        /// <summary>
        /// 
        /// </summary>
        Immediate = 0,

        /// <summary>
        /// 
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 
        /// </summary>
        Uniform = 2,

        /// <summary>
        /// 
        /// </summary>
        Late = 3,
    }

    public enum EMotionAffect
    {
        Scale = 0,
        Position = 1,
        PositionScale = 2,
        //Animation = 3,
    }
}