using System;

namespace RichTextEditorAssistViewSample
{
    public static class ServiceHelper
    {
        public static IServiceProvider Services =>
            CurrentServices ?? throw new InvalidOperationException("Services not initialized yet.");

        public static IServiceProvider? CurrentServices { get; set; }
    }
}
