using DTOs;
using UseCase;

namespace CCNAssignment2.WPFPresenters;

public class UiViewData : IViewData
{
    public UiViewData(List<IDto> data)
    {
        ViewData = data;
    }
    
    public List<IDto> ViewData { get; }
}