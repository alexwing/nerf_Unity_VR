using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load dataset/Load raw dataset")]
        static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                if (wnd != null)
                    wnd.Close();

                wnd = new RAWDatasetImporterEditorWindow(file);
                wnd.Show();
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load DICOM")]
        static void ShowDICOMImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                if (!fileCandidates.Any())
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.DisplayDialog("Could not find any DICOM files",
                        $"Failed to find any files with DICOM file extension.{Environment.NewLine}Do you want to include files without DICOM file extension?", "Yes", "No"))
                    {
                        fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    }
#endif
                }

                if (fileCandidates.Any())
                {
                    IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                    IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
                    float numVolumesCreated = 0;

                    foreach (IImageSequenceSeries series in seriesList)
                    {
                        VolumeDataset dataset = importer.ImportSeries(series);
                        if (dataset != null)
                        {
                            if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                            {
                                if (EditorUtility.DisplayDialog("Optional DownScaling",
                                    $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                                {
                                    dataset.DownScaleData();
                                }
                            }

                            VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                            obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                            numVolumesCreated++;
                        }
                    }
                }
                else
                    Debug.LogError("Could not find any DICOM files to import.");


            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

#if UNITY_EDITOR_WIN
        [MenuItem("Volume Rendering/Load dataset/Load NRRD dataset")]
        static void ShowNRRDDatasetImporter()
        {
            if (!SimpleITKManager.IsSITKEnabled())
            {
                if (EditorUtility.DisplayDialog("Missing SimpleITK", "You need to download SimpleITK to load NRRD datasets from the import settings menu.\n" +
                    "Do you want to open the import settings menu?", "Yes", "No"))
                {
                    ImportSettingsEditorWindow.ShowWindow();
                }
                return;
            }

            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nrrd)", "DataFiles", "");
            if (File.Exists(file))
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
                VolumeDataset dataset = importer.Import(file);

                if (dataset != null)
                {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }
#endif

#if UNITY_EDITOR_WIN
        [MenuItem("Volume Rendering/Load dataset/Load NIFTI dataset")]
        static void ShowNIFTIDatasetImporter()
        {
            if (!SimpleITKManager.IsSITKEnabled())
            {
                if (EditorUtility.DisplayDialog("Missing SimpleITK", "You need to download SimpleITK to load NRRD datasets from the import settings menu.\n" +
                    "Do you want to open the import settings menu?", "Yes", "No"))
                {
                    ImportSettingsEditorWindow.ShowWindow();
                }
                return;
            }

            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nii)", "DataFiles", "");
            if (File.Exists(file))
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NIFTI);
                VolumeDataset dataset = importer.Import(file);

                if (dataset != null)
                {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }
#endif

        [MenuItem("Volume Rendering/Load dataset/Load PARCHG dataset")]
        static void ShowParDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                VolumeDataset dataset = importer.Import(file);

                if (dataset != null)
                {
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }


        [MenuItem("Volume Rendering/Load dataset/Load image sequence texture 3D (Nerf Import)")]
        static void ShowSequenceImporter3DTexture()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");

            if (Directory.Exists(dir))
            {
                List<string> filePaths = Directory.GetFiles(dir).ToList();

                int size = (int)filePaths.Count;

                //Debug size of texture3D
                Debug.Log("size " + size);

                Texture3D texture3D = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
                texture3D.wrapMode = TextureWrapMode.Clamp;
                //filter mode
               // texture3D.filterMode = FilterMode.Point;
                Color32[] colors3D = new Color32[size * size * size];

                //create colors3D from texture2D from filePaths
                int i = 0;

                foreach (string filePath in filePaths)
                {
                    //import texture alpha from filepath
                    Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    //texture.filterMode = FilterMode.Point;
                    texture.LoadImage(File.ReadAllBytes(filePath));
                    Color32[] colors = texture.GetPixels32();

/*
                    //alpha cut off
                    for (int j = 0; j < colors.Length; j++)
                    {
                        if (colors[j].a < 100)
                        {
                            colors[j].a = 0;
                        }
                    }
*/
/*
                    //Fix alpha value is rgb value / 3
                    for (int j = 0; j < colors.Length; j++)
                    {
                        colors[j].a = (byte)((colors[j].r + colors[j].g + colors[j].b) / 3);
                    }

*/

                    //debug alpha value
                    Debug.Log("alpha " + colors[0].a);
                    //set colors3D from colors
                    for (int j = 0; j < colors.Length; j++)
                    {
                        colors3D[i] = colors[j];
                        i++;
                    }
                }
                //set texture3D from colors3D
                texture3D.SetPixels32(colors3D);
                texture3D.Apply();
                //save texture
                AssetDatabase.CreateAsset(texture3D, "Assets/Vol/VolumeRendering3D" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load image sequence")]
        static void ShowSequenceImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");

            if (Directory.Exists(dir))
            {
                List<string> filePaths = Directory.GetFiles(dir).ToList();
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.ImageSequence);

                IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(filePaths);

                foreach (IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = importer.ImportSeries(series);
                    if (dataset != null)
                    {
                        if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                        {
                            if (EditorUtility.DisplayDialog("Optional DownScaling",
                                $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                            {
                                dataset.DownScaleData();
                            }
                        }
                        VolumeObjectFactory.CreateObject(dataset);
                    }
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Cross section/Cross section plane")]
        static void OnMenuItemClick()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCrossSectionPlane(objects[0]);
            else
            {
                CrossSectionPlaneEditorWindow wnd = new CrossSectionPlaneEditorWindow();
                wnd.Show();
            }
        }

        [MenuItem("Volume Rendering/Cross section/Box cutout")]
        static void SpawnCutoutBox()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutBox(objects[0]);
        }

        [MenuItem("Volume Rendering/1D Transfer Function")]
        public static void Show1DTFWindow()
        {
            TransferFunctionEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/2D Transfer Function")]
        public static void Show2DTFWindow()
        {
            TransferFunction2DEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Slice renderer")]
        static void ShowSliceRenderer()
        {
            SliceRenderingEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Value range")]
        static void ShowValueRangeWindow()
        {
            ValueRangeEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Settigs")]
        static void ShowSettingsWindow()
        {
            ImportSettingsEditorWindow.ShowWindow();
        }
    }
}
