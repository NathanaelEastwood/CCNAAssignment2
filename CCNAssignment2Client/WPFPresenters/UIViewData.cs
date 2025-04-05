using Entities;

namespace CCNAssignment2.WPFPresenters;

public class UiViewData
{
    public UiViewData(List<IEntity> data)
    {
        ViewData = data;
    }
    
    public List<IEntity> ViewData { get; }
}