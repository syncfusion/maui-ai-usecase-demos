using Syncfusion.Maui.Toolkit.Charts;

namespace SmartCircularChart;

public sealed class GroupingResponseValidator
{
    public bool Validate(GroupingResponse response, out string errorMessage)
    {
        if (!Enum.IsDefined(response.GroupMode))
        {
            errorMessage = "The grouping response uses an unsupported GroupMode.";
            return false;
        }

        if (response.GroupTo is not { } groupTo)
        {
            errorMessage = string.Empty;
            return true;
        }

        if (double.IsNaN(groupTo) || double.IsInfinity(groupTo) || groupTo < 0)
        {
            errorMessage = "The grouping response must have a GroupTo of zero or more.";
            return false;
        }

        if (response.GroupMode == PieGroupMode.Percentage && groupTo > 100)
        {
            errorMessage = "A percentage GroupTo cannot be more than 100.";
            return false;
        }

        if (response.GroupMode == PieGroupMode.Angle && groupTo > 360)
        {
            errorMessage = "An angle GroupTo cannot be more than 360 degrees.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
