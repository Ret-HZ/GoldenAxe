using AceUtils.CDI;
using GoldenAxe.PDWEditor;
using GoldenAxe.PMDEditor;

namespace GoldenAxe.CDIExplorer
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
                case "PMD":
                    {
                        PMDEditorWindow pdwEditor = new PMDEditorWindow(file);
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
