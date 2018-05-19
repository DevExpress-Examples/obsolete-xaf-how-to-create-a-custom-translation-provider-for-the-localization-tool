Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.ExpressApp

Namespace GoogleTranslatorProvider
    Public NotInheritable Partial Class GoogleTranslatorProviderModule
        Inherits ModuleBase
        Public Sub New()
            InitializeComponent()
            DevExpress.ExpressApp.Utils.TranslatorProvider.RegisterProvider(New GoogleTranslatorProvider())
        End Sub
    End Class
End Namespace
