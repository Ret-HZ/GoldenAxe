using AceUtils.CDI;
using AceUtils.PDW;
using AceUtils.PMD;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoldenAxe.PMDEditor
{
    /// <summary>
    /// Interaction logic for PMDEditorWindow.xaml
    /// </summary>
    public partial class PMDEditorWindow : MetroWindow
    {
        CDIFile CDIFile;
        PMD PMD;

        string RawFilePath;
        bool IsUsingRawFile;

        public bool IsClosedForError;

        List<Element3D> SkeletonElements;
        bool IsSkeletonVisible = false;
        List<Element3D> VertexElements;
        bool IsVertexVisible = false;

        Dictionary<string, List<Element3D>> MeshElements;

        List<CDIFile> Textures;


        /// <summary>
        /// Initializes a new instance of the <see cref="PMDEditorWindow"/> class.
        /// </summary>
        /// <param name="file">The PMD file inside the CDI.</param>
        public PMDEditorWindow(CDIFile file)
        {
            InitializeComponent();
            CDIFile = file;
            PMD = PMDReader.ReadPMD(CDIFile.GetContent());
            Title = $"{Path.GetFileName(CDIFile.Name)}";
            textbox_FilePath.Text = $"{CDIFile.ParentDirectory.Name}/{CDIFile.Name}";

            InitCamera();
            PopulateTextureComboBox();
            SkeletonElements = GenerateSkeleton();
            VertexElements = GenerateVertices();
            MeshElements = GenerateMeshes();
            PopulateMeshComboBox();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PMDEditorWindow"/> class.
        /// </summary>
        /// <param name="rawFilePath">The path to the raw PMD file.</param>
        public PMDEditorWindow(string rawFilePath)
        {
            InitializeComponent();
            RawFilePath = rawFilePath;
            IsUsingRawFile = true;

            try
            {
                PMD = PMDReader.ReadPMD(File.ReadAllBytes(RawFilePath));
                Title = $"{Path.GetFileName(RawFilePath)}";
                textbox_FilePath.Text = RawFilePath;

                combobox_Textures.Visibility = Visibility.Collapsed;
                btn_TexturesBrowse.Visibility = Visibility.Visible;

                InitCamera();
                SkeletonElements = GenerateSkeleton();
                VertexElements = GenerateVertices();
                MeshElements = GenerateMeshes();
                PopulateMeshComboBox();
            }
            catch (Exception ex)
            {
                IsClosedForError = true;
                Util.ShowMessageBox($"An error has occurred while trying to read the PMD file.\n\n{ex.Message}", "Error");
                this.Close();
            }
        }


        /// <summary>
        /// Populates the "Texture" combobox with the available textures.
        /// </summary>
        private void PopulateTextureComboBox()
        {
            Textures = CDIFile.ParentDirectory.SearchFilesByName(".PDW");
            foreach (var texture in Textures)
            {
                combobox_Textures.Items.Add(texture.GetNameWithoutExtension());
            }
        }


        /// <summary>
        /// Populates the "Mesh" combobox with the available mesh names.
        /// </summary>
        private void PopulateMeshComboBox()
        {
            combobox_Meshes.Items.Add("All");
            foreach (PMDMesh mesh in PMD.Meshes)
            {
                combobox_Meshes.Items.Add(mesh.Name);
            }
            combobox_Meshes.SelectedIndex = 0;
        }


        /// <summary>
        /// Initializes the camera position, as well as attaching a directional light to it.
        /// </summary>
        private void InitCamera()
        {
            viewport3dx_ModelViewport.Camera.Position = new System.Windows.Media.Media3D.Point3D(-65.95, 19.32, -63.28);
            viewport3dx_ModelViewport.Camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(74.93, -23.70, 76.60);
            viewport3dx_ModelViewport.Camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0.003, 0.999, 0.003);
            (viewport3dx_ModelViewport.Camera as PerspectiveCamera).FarPlaneDistance = 100000;
            CameraLight.Direction = viewport3dx_ModelViewport.Camera.LookDirection;

            viewport3dx_ModelViewport.CameraChanged += (s, e) =>
            {
                var lookDirection = viewport3dx_ModelViewport.Camera.LookDirection;
                var dir = new System.Windows.Media.Media3D.Vector3D((float)lookDirection.X, (float)lookDirection.Y, (float)lookDirection.Z);
                dir.Normalize();
                CameraLight.Direction = dir;
            };
        }


        /// <summary>
        /// Generates the model's skeleton.
        /// </summary>
        /// <returns>A list of <see cref="Element3D"/> objects.</returns>
        private List<Element3D> GenerateSkeleton()
        {
            List<Element3D> elements = new List<Element3D>();

            // Bone positions
            Vector3Collection bonePositions = new Vector3Collection();
            foreach (PMDBone bone in PMD.Bones)
            {
                bonePositions.Add(new Vector3(bone.Position.Translation.X, bone.Position.Translation.Y, bone.Position.Translation.Z));
            }

            Color4Collection boneColors = new Color4Collection();
            var points = new PointGeometry3D
            {
                Positions = bonePositions,
            };

            var pointModel = new PointGeometryModel3D
            {
                Geometry = points,
                Size = new Size(10, 10),
                Figure = PointFigure.Rect,
                Color = Colors.Red,
                DepthBias = -9999
            };

            elements.Add(pointModel);


            // Bone names
            foreach (PMDBone bone in PMD.Bones)
            {
                Vector3 boneLocation = bonePositions[PMD.Bones.IndexOf(bone)];
                elements.Add(GenerateText(bone.Name, new Vector3(boneLocation.X, boneLocation.Y + 1.2f, boneLocation.Z)));
            }


            // Bone hierarchy lines
            IntCollection boneRelations = new IntCollection();
            foreach (PMDBone bone in PMD.Bones)
            {
                int selfIndex = PMD.Bones.IndexOf(bone);
                boneRelations.Add(selfIndex);
                boneRelations.Add((bone.ParentBone == null) ? selfIndex : PMD.Bones.IndexOf(bone.ParentBone));
            }

            var line = new LineGeometry3D
            {
                Positions = new Vector3Collection(bonePositions),
                Indices = boneRelations
            };
            LineGeometryModel3D lineModel3D = new LineGeometryModel3D();
            lineModel3D.Geometry = line;
            lineModel3D.Color = Colors.DarkRed;
            lineModel3D.DepthBias = -9999;
            elements.Add(lineModel3D);


            return elements;
        }


        /// <summary>
        /// Generates the model's vertex display.
        /// </summary>
        /// <returns>A list of <see cref="Element3D"/> objects.</returns>
        private List<Element3D> GenerateVertices()
        {
            List<Element3D> elements = new List<Element3D>();

            // Mesh vertices visualization
            foreach (PMDMesh mesh in PMD.Meshes)
            {
                Vector3Collection vertexPositions = new Vector3Collection();
                foreach (PMDVertex vertex in mesh.Vertices)
                {
                    vertexPositions.Add(new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                }

                var vertexPoints = new PointGeometry3D
                {
                    Positions = vertexPositions,
                };

                var vertexPointModel = new PointGeometryModel3D
                {
                    Geometry = vertexPoints,
                    Size = new Size(4, 4),
                    Figure = PointFigure.Ellipse,
                    Color = Colors.Blue
                };

                elements.Add(vertexPointModel);
            }

            return elements;
        }


        /// <summary>
        /// Generates all the meshes in the opened PMD.
        /// </summary>
        /// <returns>A dictionary of mesh names and <see cref="Element3D"/> objects.</returns>
        private Dictionary<string, List<Element3D>> GenerateMeshes()
        {
            Dictionary<string, List<Element3D>> meshElements = new Dictionary<string, List<Element3D>>();
            foreach (PMDMesh pmdMesh in PMD.Meshes)
            {
                List<Element3D> meshPartElements = new List<Element3D>();
                foreach (List<PMDVertex> vertexList in pmdMesh.Parts)
                {
                    meshPartElements.Add(GenerateMesh(vertexList));
                }
                meshElements[pmdMesh.Name] = meshPartElements;
            }
            return meshElements;
        }


        /// <summary>
        /// Generate a triangle list out of strip indices.
        /// </summary>
        /// <param name="stripIndices">The strip indices to use.</param>
        /// <returns>An <see cref="IntCollection"/>.</returns>
        IntCollection ConvertTriangleStripToTriangleList(int[] stripIndices)
        {
            var indicesList = new IntCollection();
            for (int i = 0; i < stripIndices.Length - 2; i++)
            {
                if (i % 2 == 0)
                {
                    indicesList.Add(stripIndices[i]);
                    indicesList.Add(stripIndices[i + 1]);
                    indicesList.Add(stripIndices[i + 2]);
                }
                else
                {
                    indicesList.Add(stripIndices[i + 1]);
                    indicesList.Add(stripIndices[i]);
                    indicesList.Add(stripIndices[i + 2]);
                }
            }
            return indicesList;
        }


        /// <summary>
        /// Generates the mesh geometry from a given list of vertices.
        /// </summary>
        /// <param name="vertices">The vertices to create a mesh from.</param>
        /// <returns>A <see cref="MeshGeometryModel3D"/> object.</returns>
        private MeshGeometryModel3D GenerateMesh(List<PMDVertex> vertices)
        {
            Vector3Collection vertexPositions = new Vector3Collection();
            Vector3Collection vertexNormals = new Vector3Collection();
            Vector2Collection vertexUVs = new Vector2Collection();
            foreach (var vertex in vertices)
            {
                vertexPositions.Add(new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                vertexNormals.Add(new Vector3(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
                vertexUVs.Add(new Vector2(vertex.UV.X, vertex.UV.Y));
            }

            var meshGeometry3D = new MeshGeometry3D
            {
                Positions = vertexPositions,
                Indices = ConvertTriangleStripToTriangleList(Enumerable.Range(0, vertexPositions.Count).ToArray()),
                Normals = vertexNormals,
                TextureCoordinates = vertexUVs
            };

            var material = new PhongMaterial
            {
                DiffuseColor = Color4.White,
            };

            var model = new MeshGeometryModel3D
            {
                Geometry = meshGeometry3D,
                Material = material,
            };

            return model;
        }


        /// <summary>
        /// Generates a text billboard at the specified position.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="position">The position to place it at.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>A <see cref="BillboardTextModel3D"/> object.</returns>
        private BillboardTextModel3D GenerateText(string text, Vector3 position, Color4 foreground, Color4 background, float scale)
        {
            BillboardTextModel3D textModel3D = new BillboardTextModel3D();
            BillboardSingleText3D singleText3D = new BillboardSingleText3D();
            singleText3D.TextInfo = new TextInfo
            {
                Text = text,
                Origin = position,
                Scale = scale,
                HorizontalAlignment = BillboardHorizontalAlignment.Left,
            };
            singleText3D.FontColor = foreground;
            singleText3D.BackgroundColor = background;
            singleText3D.FontFamily = "Segoe UI";
            textModel3D.Geometry = singleText3D;
            textModel3D.DepthBias = -99999;

            return textModel3D;
        }


        /// <summary>
        /// Generates a text billboard at the specified position.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="position">The position to place it at.</param>
        /// <returns>A <see cref="BillboardTextModel3D"/> object.</returns>
        private BillboardTextModel3D GenerateText(string text, Vector3 position)
        {
            return GenerateText(text, position, SharpDX.Color.Red, SharpDX.Color.Transparent, 1.2f);
        }


        /// <summary>
        /// Loaded event for the Viewport3DX. Will change the background color.
        /// </summary>
        private void viewport3dx_ModelViewport_Loaded(object sender, RoutedEventArgs e)
        {
            var renderHost = viewport3dx_ModelViewport.RenderHost;
            if (renderHost != null)
            {
                renderHost.ClearColor = SharpDX.Color.DimGray;
            }
        }


        /// <summary>
        /// Click event for the "Show/Hide bones" button. Will toggle the skeleton visibility.
        /// </summary>
        private void btn_ToggleBones_Click(object sender, RoutedEventArgs e)
        {
            if (IsSkeletonVisible)
            {
                foreach (var element in SkeletonElements)
                {
                    viewport3dx_ModelViewport.Items.Remove(element);
                }
                IsSkeletonVisible = false;
                btn_ToggleBones.Content = $"Show bones";
            }
            else
            {
                foreach (var element in SkeletonElements)
                {
                    viewport3dx_ModelViewport.Items.Add(element);
                }
                IsSkeletonVisible = true;
                btn_ToggleBones.Content = $"Hide bones";
            }
        }


        /// <summary>
        /// Click event for the "Show/Hide vertices" button. Will toggle the vertex visibility.
        /// </summary>
        private void btn_ToggleVertices_Click(object sender, RoutedEventArgs e)
        {
            if (IsVertexVisible)
            {
                foreach (var element in VertexElements)
                {
                    viewport3dx_ModelViewport.Items.Remove(element);
                }
                IsVertexVisible = false;
                btn_ToggleVertices.Content = $"Show vertices";
            }
            else
            {
                foreach (var element in VertexElements)
                {
                    viewport3dx_ModelViewport.Items.Add(element);
                }
                IsVertexVisible = true;
                btn_ToggleVertices.Content = $"Hide vertices";
            }
        }


        /// <summary>
        /// Selection changed event for the "Meshes" combobox. Will hide all meshes and only display the selected one.
        /// </summary>
        private void combobox_Meshes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (PMDMesh mesh in PMD.Meshes)
            {
                foreach (Element3D element in MeshElements[mesh.Name])
                {
                    viewport3dx_ModelViewport.Items.Remove(element);
                }
            }

            if (combobox_Meshes.SelectedIndex == 0)
            {
                foreach (PMDMesh mesh in PMD.Meshes)
                {
                    foreach (Element3D element in MeshElements[mesh.Name])
                    {
                        viewport3dx_ModelViewport.Items.Add(element);
                    }
                }
            }

            else
            {
                string selectedMesh = combobox_Meshes.SelectedItem as string;
                foreach (Element3D element in MeshElements[selectedMesh])
                {
                    viewport3dx_ModelViewport.Items.Add(element);
                }
            }
        }


        /// <summary>
        /// Selection changed event for the "Textures" combobox. Will replace the texture in all mesh materials with the selected one.
        /// </summary>
        private void combobox_Textures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTexture = combobox_Textures.SelectedItem as string;
            CDIFile textureFile = CDIFile.ParentDirectory.SearchFilesByName($"{selectedTexture}.PDW")[0];
            PDW texture = PDWReader.ReadPDW(textureFile.GetContent());

            MemoryStream stream = new MemoryStream();
            texture.Textures[0].GetBitmap().Save(stream, ImageFormat.Bmp);
            stream.Position = 0;

            var material = new PhongMaterial
            {
                DiffuseColor = Color4.White,
                DiffuseMap = new TextureModel(stream)
            };

            foreach (PMDMesh mesh in PMD.Meshes)
            {
                foreach (Element3D partElement in MeshElements[mesh.Name])
                {
                    MeshGeometryModel3D meshGeometry = partElement as MeshGeometryModel3D;
                    meshGeometry.Material = material;
                }
            }
        }


        /// <summary>
        /// Click event for the "Textures - Browse" button. Will open a file dialog and replace the texture in all mesh materials with the selected one.
        /// </summary>
        private void btn_TexturesBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDW Textures (*.PDW)|*.PDW";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    PDW texture = PDWReader.ReadPDW(File.ReadAllBytes(openFileDialog.FileName));

                    MemoryStream stream = new MemoryStream();
                    texture.Textures[0].GetBitmap().Save(stream, ImageFormat.Bmp);
                    stream.Position = 0;

                    var material = new PhongMaterial
                    {
                        DiffuseColor = Color4.White,
                        DiffuseMap = new TextureModel(stream)
                    };

                    foreach (PMDMesh mesh in PMD.Meshes)
                    {
                        foreach (Element3D partElement in MeshElements[mesh.Name])
                        {
                            MeshGeometryModel3D meshGeometry = partElement as MeshGeometryModel3D;
                            meshGeometry.Material = material;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.ShowMessageBox(this, $"An error has occurred while trying to read the texture file.\n\n{ex.Message}", "Error");
                }
            }
        }
    }



    public class PMDEditorViewModel : INotifyPropertyChanged
    {
        public EffectsManager EffectsManager { get; }

        public HelixToolkit.Wpf.SharpDX.Camera Camera { get; }

        public PMDEditorViewModel()
        {
            EffectsManager = new DefaultEffectsManager();
            Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        protected bool Set<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
