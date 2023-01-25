using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace IterativeClosestPointTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int mode = 3;
            if (mode == 1)
                IterativeClosestPointTest1();
            else if (mode == 2)
                IterativeClosestPointTest2();
            else
                IterativeClosestPointTest3();
        }

        public static void IterativeClosestPointTest1()
        {
            vtkPoints pPoints1 = vtkPoints.New();
            pPoints1.InsertNextPoint(-1.5, 0, 0);
            pPoints1.InsertNextPoint(1.5, 0, 0);
            pPoints1.InsertNextPoint(0, 1, 0);

            vtkCellArray pPolys1 = vtkCellArray.New();
            pPolys1.InsertNextCell(3); // number of points
            pPolys1.InsertCellPoint(0);  // Point's ID
            pPolys1.InsertCellPoint(1);
            pPolys1.InsertCellPoint(2);


            vtkPolyData pPolyData1 = vtkPolyData.New();
            pPolyData1.SetPoints(pPoints1);
            pPolyData1.SetPolys(pPolys1);

            vtkPoints pPoints2 = vtkPoints.New();
            pPoints2.InsertNextPoint(4, 2, 0);
            pPoints2.InsertNextPoint(2, 4, 0);
            pPoints2.InsertNextPoint(2, 2, 0);

            vtkCellArray pPolys2 = vtkCellArray.New();
            pPolys2.InsertNextCell(3); // number of points
            pPolys2.InsertCellPoint(0);  // Point's ID
            pPolys2.InsertCellPoint(1);
            pPolys2.InsertCellPoint(2);


            vtkPolyData pPolyData2 = vtkPolyData.New();
            pPolyData2.SetPoints(pPoints2);
            pPolyData2.SetPolys(pPolys2);

            vtkPolyDataMapper mapper1 = vtkPolyDataMapper.New();
            mapper1.SetInput(pPolyData1);
            mapper1.Update();

            vtkActor actor1 = vtkActor.New();
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetRepresentationToWireframe();
            actor1.GetProperty().SetColor(1, 0, 0);

            vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
            mapper2.SetInput(pPolyData2);
            mapper2.Update();

            vtkActor actor2 = vtkActor.New();
            actor2.SetMapper(mapper2);
            actor2.GetProperty().SetRepresentationToWireframe();
            actor2.GetProperty().SetColor(0, 1, 0);

            vtkRenderer renderer = vtkRenderer.New();
            renderer.AddActor(actor1);
            renderer.AddActor(actor2);
            renderer.SetBackground(.1, .2, .3);
            renderer.ResetCamera();

            vtkRenderWindow renderWin = vtkRenderWindow.New();
            renderWin.AddRenderer(renderer);

            vtkRenderWindowInteractor interactor = vtkRenderWindowInteractor.New();
            interactor.SetRenderWindow(renderWin);

            renderWin.Render();

            System.Threading.Thread.Sleep(2000); // 2 second

            vtkLandmarkTransform lmt = vtkLandmarkTransform.New();
            lmt.SetSourceLandmarks(pPoints1);
            lmt.SetTargetLandmarks(pPoints2);
            lmt.SetModeToRigidBody();
            lmt.Update();

            actor1.SetUserTransform(lmt);
            renderWin.Render();

            interactor.Start();
        }

        public static void IterativeClosestPointTest2()
        {
            vtkSTLReader pSTLReader1 = vtkSTLReader.New();
            pSTLReader1.SetFileName("../../../../res/cow.stl");
            pSTLReader1.Update();

            vtkSTLReader pSTLReader2 = vtkSTLReader.New();
            pSTLReader2.SetFileName("../../../../res/cowTrans.stl");
            pSTLReader2.Update();

            vtkPolyDataMapper mapper1 = vtkPolyDataMapper.New();
            mapper1.SetInputConnection(pSTLReader1.GetOutputPort());

            vtkActor actor1 = vtkActor.New();
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetColor(1.0, 1.0, 0.5);
            actor1.GetProperty().SetOpacity(0.5);

            vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
            mapper2.SetInputConnection(pSTLReader2.GetOutputPort());

            vtkActor actor2 = vtkActor.New();
            actor2.SetMapper(mapper2);
            actor2.GetProperty().SetOpacity(0.5);

            vtkRenderer renderer = vtkRenderer.New();
            renderer.AddActor(actor1);
            renderer.AddActor(actor2);
            renderer.SetBackground(.1, .2, .3);
            renderer.ResetCamera();

            vtkRenderWindow renderWin = vtkRenderWindow.New();
            renderWin.AddRenderer(renderer);

            vtkRenderWindowInteractor interactor = vtkRenderWindowInteractor.New();
            interactor.SetRenderWindow(renderWin);

            renderWin.Render();

            System.Threading.Thread.Sleep(2000); // 2 second

            vtkIterativeClosestPointTransform ICP = vtkIterativeClosestPointTransform.New();
            ICP.SetSource(pSTLReader1.GetOutput());
            ICP.SetTarget(pSTLReader2.GetOutput());
            ICP.GetLandmarkTransform().SetModeToRigidBody();
            ICP.SetMaximumNumberOfIterations(100);  // default 50
            ICP.SetMaximumNumberOfLandmarks(50); // default 200
            ICP.Update();

            actor1.SetUserTransform(ICP);
            renderWin.Render();
            interactor.Start();
        }

        public static void IterativeClosestPointTest3()
        {
            vtkDICOMImageReader dcmReader = vtkDICOMImageReader.New();
            dcmReader.SetDirectoryName("../../../../res/CT");
            dcmReader.Update();

            vtkMarchingCubes pMatchingCube = vtkMarchingCubes.New();
            pMatchingCube.SetInputConnection(dcmReader.GetOutputPort());
            pMatchingCube.SetValue(0, -500); // iso value
            pMatchingCube.ComputeScalarsOff();
            pMatchingCube.ComputeNormalsOn();
            pMatchingCube.Update();

            vtkStripper skinStripper = vtkStripper.New();
            skinStripper.SetInputConnection(pMatchingCube.GetOutputPort());


            vtkPolyDataMapper mapper1 = vtkPolyDataMapper.New();
            mapper1.SetInputConnection(skinStripper.GetOutputPort());

            vtkActor actor1 = vtkActor.New();
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetDiffuseColor(1, 1, 1); // (.1, .94, .52);
            actor1.GetProperty().SetSpecular(.3);
            actor1.GetProperty().SetSpecularPower(20);
            actor1.GetProperty().SetOpacity(0.5);

            vtkMarchingCubes pMatchingCube2 = vtkMarchingCubes.New();
            pMatchingCube2.SetInputConnection(dcmReader.GetOutputPort());
            pMatchingCube2.SetValue(0, 330); // iso value  333
            pMatchingCube2.ComputeScalarsOff();
            pMatchingCube2.ComputeNormalsOn();
            pMatchingCube2.Update();

            vtkStripper boneStripper2 = vtkStripper.New();
            boneStripper2.SetInputConnection(pMatchingCube2.GetOutputPort());


            vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
            mapper2.SetInputConnection(boneStripper2.GetOutputPort());

            vtkActor actor2 = vtkActor.New();
            actor2.SetMapper(mapper2);

            vtkRenderer renderer = vtkRenderer.New();
            renderer.AddActor(actor1);
            renderer.AddActor(actor2);
            renderer.SetBackground(.1, .2, .3);
            renderer.ResetCamera();

            vtkRenderWindow renderWin = vtkRenderWindow.New();
            renderWin.AddRenderer(renderer);

            vtkRenderWindowInteractor interactor = vtkRenderWindowInteractor.New();
            interactor.SetRenderWindow(renderWin);

            renderWin.Render();

            System.Threading.Thread.Sleep(2000); // 2 second

            vtkIterativeClosestPointTransform ICP = vtkIterativeClosestPointTransform.New();
            ICP.SetSource(skinStripper.GetOutput());
            ICP.SetTarget(boneStripper2.GetOutput());
            ICP.GetLandmarkTransform().SetModeToRigidBody();
            ICP.SetMaximumNumberOfIterations(100);  // default 50
            ICP.SetMaximumNumberOfLandmarks(50); // default 200
            ICP.Update();

            actor1.SetUserTransform(ICP);
            renderWin.Render();
            interactor.Start();
        }
    }
}
