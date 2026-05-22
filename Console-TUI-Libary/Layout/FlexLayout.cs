using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Layout;

public class FlexLayout
{
    public enum Direction
    {
        Row,
        Column
    }

    public Direction Flow { get; set; } = Direction.Column;

    public int Spacing { get; set; } = 0;
}