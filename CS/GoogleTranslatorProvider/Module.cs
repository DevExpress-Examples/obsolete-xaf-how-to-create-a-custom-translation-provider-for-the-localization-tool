using System;
using DevExpress.ExpressApp;

namespace GoogleTranslatorProvider {
    public sealed partial class GoogleTranslatorProviderModule : ModuleBase {
        public GoogleTranslatorProviderModule() {
            InitializeComponent();
            DevExpress.ExpressApp.Utils.TranslatorProvider.RegisterProvider(new GoogleTranslatorProvider());
        }
    }
}
