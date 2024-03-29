using AceUtils.CDI;
using Golden_Axe.PDWEditor;

namespace Golden_Axe.CDIExplorer
{
    public static class FileEditorHandler
    {
        public static void OpenFileEditor(CDIFile file)
        {
            switch (file.GetExtension())
            {
                case "PDW":
                    {
                        PDWEditorWindow pdwEditor = new PDWEditorWindow(file);
                        pdwEditor.Show();
                        break;
                    }
                default:
                    {
                        Util.ShowMessageBox("No editor available for this format.", "Error");
                        break;
                    }
            }
        }
    }
}
