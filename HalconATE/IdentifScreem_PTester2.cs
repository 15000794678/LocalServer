//
//  File generated by HDevelop for HALCON/DOTNET (C#) Version 10.0
//

using HalconDotNet;

namespace IdentifScreem_PTester2
{
    public partial class HDevelopExport
    {
#if !NO_EXPORT_APP_MAIN
        public HDevelopExport()
        {
            // Default settings used in HDevelop 
            HOperatorSet.SetSystem("do_low_error", "false");
            action();
        }
#endif

        // Procedures 
        // Local procedures 
        public void ConnectCamera(out HTuple hv_AcqHandle)
        {
            // Initialize local and output iconic variables 

            HOperatorSet.OpenFramegrabber("GigEVision", 0, 0, 0, 0, 0, 0, "progressive",
                -1, "default", -1, "false", "default", "0007486f017b_TheImagingSourceEuropeGmbH_DMK23G2",
                0, -1, out hv_AcqHandle);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "ExposureTime", 2000.0);


            return;
        }

        public void DisconnectCamera(HTuple hv_AcqHandle)
        {

            // Initialize local and output iconic variables 

            HOperatorSet.CloseFramegrabber(hv_AcqHandle);

            return;
        }

        public void get_image_from_camera(out HObject ho_Image, HTuple hv_AcqHandle, HTuple hv_ImageNum)
        {

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);

            ho_Image.Dispose();
            HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, -1);

            return;
        }

        public void do_inspect(HObject ho_Image, HTuple hv_BoxModelPath, out HTuple hv_outport_result)
        {



            // Local control variables 

            HTuple hv_Width, hv_Height, hv_BoxModelID;
            HTuple hv_BoxModelRow, hv_BoxModelColumn, hv_BoxModelAngle;
            HTuple hv_BoxModelScore;

            // Initialize local and output iconic variables 

            hv_outport_result = new HTuple();
            //read_image (Image, fileName)
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            HOperatorSet.ReadNccModel(hv_BoxModelPath, out hv_BoxModelID);
            HOperatorSet.FindNccModel(ho_Image, hv_BoxModelID, 0, 0, 0.5, 1, 0.5, "true",
                0, out hv_BoxModelRow, out hv_BoxModelColumn, out hv_BoxModelAngle, out hv_BoxModelScore);
            if ((int)((new HTuple((new HTuple(hv_BoxModelScore.TupleLength())).TupleEqual(
                0))).TupleOr(new HTuple(((hv_BoxModelScore.TupleSelect(0))).TupleLess(0.75)))) != 0)
            {
                hv_outport_result = new HTuple();
                hv_outport_result[0] = 0;
                hv_outport_result[1] = 0;
                hv_outport_result[2] = 0;
            }
            else
            {
                hv_outport_result[0] = 1;
                hv_outport_result[1] = ((hv_BoxModelRow.TupleSelect(0))).TupleInt();
                hv_outport_result[2] = ((hv_BoxModelColumn.TupleSelect(0))).TupleInt();
            }
            HOperatorSet.ClearNccModel(hv_BoxModelID);

            return;
        }

        public void get_image_from_file(ref HObject ho_Image, HTuple hv_filename)
        {


            // Local control variables 

            // Initialize local and output iconic variables 
            //HOperatorSet.GenEmptyObj(out ho_Image);

            ho_Image.Dispose();
            HOperatorSet.ReadImage(out ho_Image, hv_filename);
            //get_image_size (Image, Width, Height)

            return;
        }

        public void do_inspect2(HTuple hv_fileName, out HTuple hv_outport_result)
        {


            // Local iconic variables 

            HObject ho_Image, ho_Image1 = null, ho_Regions;
            HObject ho_ConnectedRegions, ho_SelectedRegions, ho_SelectedRegions1;


            // Local control variables 

            HTuple hv_Width, hv_Height, hv_Rectangularity;
            HTuple hv_Area, hv_ModelRow, hv_ModelColumn, hv_FindModelRowNum = new HTuple();
            HTuple hv_FindModelColumnNum = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Image1);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);

            hv_outport_result = new HTuple();
            ho_Image.Dispose();
            HOperatorSet.ReadImage(out ho_Image, hv_fileName);
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_Image1, out ho_Regions, 0, 2);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            HOperatorSet.Rectangularity(ho_ConnectedRegions, out hv_Rectangularity);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "Rectangularity",
                "and", 0.8, 1);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions, out ho_SelectedRegions1, "area",
                "and", 130000, 158720);
            HOperatorSet.AreaCenter(ho_SelectedRegions1, out hv_Area, out hv_ModelRow, out hv_ModelColumn);

            if ((int)(new HTuple((new HTuple(hv_Area.TupleLength())).TupleEqual(0))) != 0)
            {
                hv_outport_result = 0;
            }
            else
            {
                hv_outport_result[0] = new HTuple(hv_Area.TupleLength());
                hv_outport_result[1] = hv_Width.TupleInt();
                hv_outport_result[2] = hv_Height.TupleInt();
                for (hv_FindModelRowNum = 3; (int)hv_FindModelRowNum <= (int)((new HTuple(hv_Area.TupleLength()
                    )) + 2); hv_FindModelRowNum = (int)hv_FindModelRowNum + 1)
                {
                    hv_outport_result[hv_FindModelRowNum] = ((hv_ModelRow.TupleSelect(hv_FindModelRowNum - 3))).TupleInt()
                        ;
                }
                for (hv_FindModelColumnNum = (new HTuple(hv_Area.TupleLength())) + 3; (int)hv_FindModelColumnNum <= (int)(((new HTuple(hv_Area.TupleLength()
                    )) + (new HTuple(hv_Area.TupleLength()))) + 2); hv_FindModelColumnNum = (int)hv_FindModelColumnNum + 1)
                {
                    hv_outport_result[hv_FindModelColumnNum] = ((hv_ModelColumn.TupleSelect((hv_FindModelColumnNum - (new HTuple(hv_Area.TupleLength()
                        ))) - 3))).TupleInt();
                }
            }


            ho_Image.Dispose();
            ho_Image1.Dispose();
            ho_Regions.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SelectedRegions1.Dispose();

            return;
        }

        public void do_inspect1(HObject ho_Image, HTuple hv_countPath, HTuple hv_startPath,
            HTuple hv_pausePath, HTuple hv_stopPath, out HTuple hv_outport_result)
        {



            // Local iconic variables 

            HObject ho_rightRectangle = null, ho_leftRectangle = null;
            HObject ho_GrayImage = null, ho_rightImageReduced = null, ho_leftImageReduced = null;
            HObject ho_leftBlueRegions = null, ho_rightBlueRegions = null;
            HObject ho_leftRedRegions = null, ho_rightRedRegions = null;
            HObject ho_leftYellowRegions = null, ho_rightYellowRegions = null;
            HObject ho_leftGreenRegions = null, ho_rightGreenRegions = null;


            // Local control variables 

            HTuple hv_TestModelID, hv_testRow, hv_testColumn;
            HTuple hv_testAngle, hv_testScore, hv_rightBlueArea = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_leftBlueArea = new HTuple();
            HTuple hv_rightRedArea = new HTuple(), hv_leftRedArea = new HTuple();
            HTuple hv_rightYellowArea = new HTuple(), hv_leftYellowArea = new HTuple();
            HTuple hv_rightGreenArea = new HTuple(), hv_leftGreenArea = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_rightRectangle);
            HOperatorSet.GenEmptyObj(out ho_leftRectangle);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_rightImageReduced);
            HOperatorSet.GenEmptyObj(out ho_leftImageReduced);
            HOperatorSet.GenEmptyObj(out ho_leftBlueRegions);
            HOperatorSet.GenEmptyObj(out ho_rightBlueRegions);
            HOperatorSet.GenEmptyObj(out ho_leftRedRegions);
            HOperatorSet.GenEmptyObj(out ho_rightRedRegions);
            HOperatorSet.GenEmptyObj(out ho_leftYellowRegions);
            HOperatorSet.GenEmptyObj(out ho_rightYellowRegions);
            HOperatorSet.GenEmptyObj(out ho_leftGreenRegions);
            HOperatorSet.GenEmptyObj(out ho_rightGreenRegions);

            hv_outport_result = new HTuple();


            HOperatorSet.ReadNccModel(hv_countPath, out hv_TestModelID);

            HOperatorSet.FindNccModel(ho_Image, hv_TestModelID, -0.39, 0.78, 0.5, 2, 0.5,
                "true", 3, out hv_testRow, out hv_testColumn, out hv_testAngle, out hv_testScore);
            if ((int)((new HTuple((new HTuple(hv_testScore.TupleLength())).TupleEqual(2))).TupleAnd(
                new HTuple(((hv_testScore.TupleSelect(1))).TupleGreater(0.75)))) != 0)
            {
                if ((int)(new HTuple(((hv_testColumn.TupleSelect(0))).TupleGreater(hv_testColumn.TupleSelect(
                    1)))) != 0)
                {
                    ho_rightRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_rightRectangle, (hv_testRow.TupleSelect(
                        0)) - 12, (hv_testColumn.TupleSelect(0)) - (((hv_testColumn.TupleSelect(0)) - (hv_testColumn.TupleSelect(
                        1))) - 120), (hv_testRow.TupleSelect(0)) + 38, (hv_testColumn.TupleSelect(
                        0)) - 30);
                    ho_leftRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_leftRectangle, (hv_testRow.TupleSelect(
                        1)) - 12, (hv_testColumn.TupleSelect(1)) - (((hv_testColumn.TupleSelect(0)) - (hv_testColumn.TupleSelect(
                        1))) - 120), (hv_testRow.TupleSelect(1)) + 38, (hv_testColumn.TupleSelect(
                        1)) - 30);
                }
                else
                {
                    ho_rightRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_rightRectangle, (hv_testRow.TupleSelect(
                        1)) - 12, (hv_testColumn.TupleSelect(1)) - (((hv_testColumn.TupleSelect(1)) - (hv_testColumn.TupleSelect(
                        0))) - 120), (hv_testRow.TupleSelect(1)) + 38, (hv_testColumn.TupleSelect(
                        1)) - 30);
                    ho_leftRectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_leftRectangle, (hv_testRow.TupleSelect(
                        0)) - 12, (hv_testColumn.TupleSelect(0)) - (((hv_testColumn.TupleSelect(1)) - (hv_testColumn.TupleSelect(
                        0))) - 120), (hv_testRow.TupleSelect(0)) + 38, (hv_testColumn.TupleSelect(
                        0)) - 30);
                }
                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                ho_rightImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_GrayImage, ho_rightRectangle, out ho_rightImageReduced
                    );
                ho_leftImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_GrayImage, ho_leftRectangle, out ho_leftImageReduced
                    );

                //��ɫstart
                ho_leftBlueRegions.Dispose();
                HOperatorSet.Threshold(ho_leftImageReduced, out ho_leftBlueRegions, 29, 29);
                ho_rightBlueRegions.Dispose();
                HOperatorSet.Threshold(ho_rightImageReduced, out ho_rightBlueRegions, 29, 29);
                HOperatorSet.AreaCenter(ho_rightBlueRegions, out hv_rightBlueArea, out hv_Row,
                    out hv_Column);
                HOperatorSet.AreaCenter(ho_leftBlueRegions, out hv_leftBlueArea, out hv_Row,
                    out hv_Column);

                //��ɫfail
                ho_leftRedRegions.Dispose();
                HOperatorSet.Threshold(ho_leftImageReduced, out ho_leftRedRegions, 76, 76);
                ho_rightRedRegions.Dispose();
                HOperatorSet.Threshold(ho_rightImageReduced, out ho_rightRedRegions, 76, 76);
                HOperatorSet.AreaCenter(ho_rightRedRegions, out hv_rightRedArea, out hv_Row,
                    out hv_Column);
                HOperatorSet.AreaCenter(ho_leftRedRegions, out hv_leftRedArea, out hv_Row,
                    out hv_Column);


                //��ɫtest
                ho_leftYellowRegions.Dispose();
                HOperatorSet.Threshold(ho_leftImageReduced, out ho_leftYellowRegions, 226,
                    226);
                ho_rightYellowRegions.Dispose();
                HOperatorSet.Threshold(ho_rightImageReduced, out ho_rightYellowRegions, 226,
                    226);
                HOperatorSet.AreaCenter(ho_rightYellowRegions, out hv_rightYellowArea, out hv_Row,
                    out hv_Column);
                HOperatorSet.AreaCenter(ho_leftYellowRegions, out hv_leftYellowArea, out hv_Row,
                    out hv_Column);

                //��ɫpass
                ho_leftGreenRegions.Dispose();
                HOperatorSet.Threshold(ho_leftImageReduced, out ho_leftGreenRegions, 150, 150);
                ho_rightGreenRegions.Dispose();
                HOperatorSet.Threshold(ho_rightImageReduced, out ho_rightGreenRegions, 150,
                    150);
                HOperatorSet.AreaCenter(ho_rightGreenRegions, out hv_rightGreenArea, out hv_Row,
                    out hv_Column);
                HOperatorSet.AreaCenter(ho_leftGreenRegions, out hv_leftGreenArea, out hv_Row,
                    out hv_Column);


                hv_outport_result[0] = 5;
                //left
                if ((int)((new HTuple((new HTuple(hv_leftGreenArea.TupleLength())).TupleEqual(
                    1))).TupleAnd(new HTuple(hv_leftGreenArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[1] = 2;
                }
                else if ((int)((new HTuple((new HTuple(hv_leftBlueArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_leftBlueArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[1] = 0;
                }
                else if ((int)((new HTuple((new HTuple(hv_leftYellowArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_leftYellowArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[1] = 1;
                }
                else if ((int)((new HTuple((new HTuple(hv_leftRedArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_leftRedArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[1] = 3;
                }
                else
                {
                    hv_outport_result[1] = 4;
                }
                //right
                if ((int)((new HTuple((new HTuple(hv_rightGreenArea.TupleLength())).TupleEqual(
                    1))).TupleAnd(new HTuple(hv_rightGreenArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[2] = 2;
                }
                else if ((int)((new HTuple((new HTuple(hv_rightBlueArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_rightBlueArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[2] = 0;
                }
                else if ((int)((new HTuple((new HTuple(hv_rightYellowArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_rightYellowArea.TupleGreater(
                    400)))) != 0)
                {
                    hv_outport_result[2] = 1;
                }
                else if ((int)((new HTuple((new HTuple(hv_rightRedArea.TupleLength()
                    )).TupleEqual(1))).TupleAnd(new HTuple(hv_rightRedArea.TupleGreater(400)))) != 0)
                {
                    hv_outport_result[2] = 3;
                }
                else
                {
                    hv_outport_result[2] = 4;
                }

            }
            else
            {
                hv_outport_result[0] = 3;
            }
            HOperatorSet.ClearNccModel(hv_TestModelID);
            ho_rightRectangle.Dispose();
            ho_leftRectangle.Dispose();
            ho_GrayImage.Dispose();
            ho_rightImageReduced.Dispose();
            ho_leftImageReduced.Dispose();
            ho_leftBlueRegions.Dispose();
            ho_rightBlueRegions.Dispose();
            ho_leftRedRegions.Dispose();
            ho_rightRedRegions.Dispose();
            ho_leftYellowRegions.Dispose();
            ho_rightYellowRegions.Dispose();
            ho_leftGreenRegions.Dispose();
            ho_rightGreenRegions.Dispose();

            return;
        }

        public void do_inspect4(HObject ho_Image, HTuple hv_failPath, HTuple hv_passPath,
            out HTuple hv_outport_result)
        {


            // Initialize local and output iconic variables 

            hv_outport_result = new HTuple();

        }

        // Main procedure 
        private void action()
        {

            // Local iconic variables 


            // Local control variables 

            // Initialize local and output iconic variables 



            //get_image_from_file (Image, 'C:/Users/shen/Desktop/model/success.bmp')
            //do_inspect1 (Image, 'C:/Users/shen/Desktop/model/testover.ncm', pausePath, startPath, stopPath, outport_result)




            return;

        }


    }
#if !NO_EXPORT_APP_MAIN
    public class HDevelopExportApp
    {
        //static void Main(string[] args)
        //{
        //  new HDevelopExport();
        //}
    }
#endif
}

