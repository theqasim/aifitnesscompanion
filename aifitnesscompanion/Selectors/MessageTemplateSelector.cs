
using static aifitnesscompanion.ViewModels.AIFitnessChatbotViewModel;

namespace aifitnesscompanion.Selectors
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserMessageTemplate { get; set; }
        public DataTemplate AIMessageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (!(item is MessageViewModel messageViewModel))
                return null;

            return messageViewModel.Role == "User" ? UserMessageTemplate : AIMessageTemplate;
        }
    }
}

