﻿//
//  File generated by HDevelop for HALCON/DOTNET (C#) Version 10.0
//

using HalconDotNet;

namespace IdentifScreem_LuSen
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
                //hv_outport_result[1] = ((((hv_BoxModelRow.TupleSelect(0))*65535)/hv_Height)).TupleInt()
                //    ;
                //hv_outport_result[2] = ((((hv_BoxModelColumn.TupleSelect(0))*65535)/hv_Width)).TupleInt()
                //    ;
                hv_outport_result[1] = hv_BoxModelRow.TupleSelect(0).TupleInt()
                    ;
                hv_outport_result[2] = hv_BoxModelColumn.TupleSelect(0).TupleInt()
                    ;

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

        public void do_inspect1(HObject ho_Image, HTuple hv_RightModelPath, HTuple hv_TestModelPath,
            out HTuple hv_outport_result)
        {



            // Local iconic variables 

            HObject ho_UpRectangle = null, ho_UpImageReduced = null;
            HObject ho_DownRectangle = null, ho_DownImageReduced = null;
            HObject ho_UpSuccessRegions = null, ho_UpSuccessConnectedRegions = null;
            HObject ho_UpSuccessSelectedRegions = null, ho_UpSuccessSelectedRegions1 = null;
            HObject ho_UpFailRegions = null, ho_UpFailConnectedRegions = null;
            HObject ho_UpFailSelectedRegions = null, ho_UpFailSelectedRegions1 = null;
            HObject ho_DownSuccessRegions = null, ho_DownSuccessConnectedRegions = null;
            HObject ho_DownSuccessSelectedRegions = null, ho_DownSuccessSelectedRegions1 = null;
            HObject ho_DownFailRegions = null, ho_DownFailConnectedRegions = null;
            HObject ho_DownFailSelectedRegions = null, ho_DownFailSelectedRegions1 = null;


            // Local control variables 

            HTuple hv_Width, hv_Height, hv_RightModelID;
            HTuple hv_RightModelRow, hv_RightModelColumn, hv_RightModelAngle;
            HTuple hv_RightModelScore, hv_TestModelID = new HTuple();
            HTuple hv_UpTestRow = new HTuple(), hv_UpTestColumn = new HTuple();
            HTuple hv_UpTestAngle = new HTuple(), hv_UpTestScore = new HTuple();
            HTuple hv_DownTestRow = new HTuple(), hv_DownTestColumn = new HTuple();
            HTuple hv_DownTestAngle = new HTuple(), hv_DownTestScore = new HTuple();
            HTuple hv_upStartStateArea = new HTuple(), hv_upStartStateRow = new HTuple();
            HTuple hv_upStartStateColumn = new HTuple(), hv_UpSuccessArea = new HTuple();
            HTuple hv_UpSuccessRow = new HTuple(), hv_UpSuccessColumn = new HTuple();
            HTuple hv_UpFailArea = new HTuple(), hv_UpFailRow = new HTuple();
            HTuple hv_UpFailColumn = new HTuple(), hv_downStartStateArea = new HTuple();
            HTuple hv_downStartStateRow = new HTuple(), hv_downStartStateColumn = new HTuple();
            HTuple hv_DownSuccessArea = new HTuple(), hv_DownSuccessRow = new HTuple();
            HTuple hv_DownSuccessColumn = new HTuple(), hv_DownFailArea = new HTuple();
            HTuple hv_DownFailRow = new HTuple(), hv_DownFailColumn = new HTuple();
            HTuple hv_outport_upresult = new HTuple(), hv_outport_downresult = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_UpRectangle);
            HOperatorSet.GenEmptyObj(out ho_UpImageReduced);
            HOperatorSet.GenEmptyObj(out ho_DownRectangle);
            HOperatorSet.GenEmptyObj(out ho_DownImageReduced);
            HOperatorSet.GenEmptyObj(out ho_UpSuccessRegions);
            HOperatorSet.GenEmptyObj(out ho_UpSuccessConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_UpSuccessSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_UpSuccessSelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_UpFailRegions);
            HOperatorSet.GenEmptyObj(out ho_UpFailConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_UpFailSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_UpFailSelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_DownSuccessRegions);
            HOperatorSet.GenEmptyObj(out ho_DownSuccessConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DownSuccessSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DownSuccessSelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_DownFailRegions);
            HOperatorSet.GenEmptyObj(out ho_DownFailConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DownFailSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DownFailSelectedRegions1);

            hv_outport_result = new HTuple();
            //read_image (Image, fileName)
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            HOperatorSet.ReadNccModel(hv_RightModelPath, out hv_RightModelID);
            HOperatorSet.FindNccModel(ho_Image, hv_RightModelID, 0, 0, 0.5, 1, 0.5, "true",
                0, out hv_RightModelRow, out hv_RightModelColumn, out hv_RightModelAngle,
                out hv_RightModelScore);
            if ((int)((new HTuple((new HTuple(hv_RightModelScore.TupleLength())).TupleEqual(
                0))).TupleOr(new HTuple(((hv_RightModelScore.TupleSelect(0))).TupleLess(0.75)))) != 0)
            {
                //û�ҵ���λ�����Ҳ���԰�ť��
                hv_outport_result = new HTuple();
                hv_outport_result[0] = 4;
                hv_outport_result[1] = 4;
            }
            else
            {
                ho_UpRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_UpRectangle, (hv_RightModelRow.TupleSelect(
                    0)) - 305, (hv_RightModelColumn.TupleSelect(0)) - 815, (hv_RightModelRow.TupleSelect(
                    0)) - 50, (hv_RightModelColumn.TupleSelect(0)) - 70);
                ho_UpImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_UpRectangle, out ho_UpImageReduced);
                ho_DownRectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_DownRectangle, (hv_RightModelRow.TupleSelect(
                    0)) - 30, (hv_RightModelColumn.TupleSelect(0)) - 815, (hv_RightModelRow.TupleSelect(
                    0)) + 230, (hv_RightModelColumn.TupleSelect(0)) - 70);
                ho_DownImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_DownRectangle, out ho_DownImageReduced
                    );

                HOperatorSet.ReadNccModel(hv_TestModelPath, out hv_TestModelID);
                HOperatorSet.FindNccModel(ho_UpImageReduced, hv_TestModelID, 0, 0, 0.5, 1,
                    0.5, "true", 0, out hv_UpTestRow, out hv_UpTestColumn, out hv_UpTestAngle,
                    out hv_UpTestScore);
                HOperatorSet.FindNccModel(ho_DownImageReduced, hv_TestModelID, 0, 0, 0.5, 1,
                    0.5, "true", 0, out hv_DownTestRow, out hv_DownTestColumn, out hv_DownTestAngle,
                    out hv_DownTestScore);

                ho_UpSuccessRegions.Dispose();
                HOperatorSet.Threshold(ho_UpImageReduced, out ho_UpSuccessRegions, 0, 2);
                HOperatorSet.AreaCenter(ho_UpSuccessRegions, out hv_upStartStateArea, out hv_upStartStateRow,
                    out hv_upStartStateColumn);

                ho_UpSuccessConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_UpSuccessRegions, out ho_UpSuccessConnectedRegions
                    );
                ho_UpSuccessSelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_UpSuccessConnectedRegions, out ho_UpSuccessSelectedRegions,
                    "rectangularity", "and", 0.8, 1);
                ho_UpSuccessSelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_UpSuccessSelectedRegions, out ho_UpSuccessSelectedRegions1,
                    "area", "and", 10000, 158720);
                HOperatorSet.AreaCenter(ho_UpSuccessSelectedRegions1, out hv_UpSuccessArea,
                    out hv_UpSuccessRow, out hv_UpSuccessColumn);

                ho_UpFailRegions.Dispose();
                HOperatorSet.Threshold(ho_UpImageReduced, out ho_UpFailRegions, 138, 140);
                ho_UpFailConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_UpFailRegions, out ho_UpFailConnectedRegions);
                ho_UpFailSelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_UpFailConnectedRegions, out ho_UpFailSelectedRegions,
                    "rectangularity", "and", 0.8, 1);
                ho_UpFailSelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_UpFailSelectedRegions, out ho_UpFailSelectedRegions1,
                    "area", "and", 10000, 158720);
                HOperatorSet.AreaCenter(ho_UpFailSelectedRegions1, out hv_UpFailArea, out hv_UpFailRow,
                    out hv_UpFailColumn);



                ho_DownSuccessRegions.Dispose();
                HOperatorSet.Threshold(ho_DownImageReduced, out ho_DownSuccessRegions, 0, 2);
                HOperatorSet.AreaCenter(ho_DownSuccessRegions, out hv_downStartStateArea, out hv_downStartStateRow,
                    out hv_downStartStateColumn);

                ho_DownSuccessConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_DownSuccessRegions, out ho_DownSuccessConnectedRegions
                    );
                ho_DownSuccessSelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_DownSuccessConnectedRegions, out ho_DownSuccessSelectedRegions,
                    "rectangularity", "and", 0.8, 1);
                ho_DownSuccessSelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_DownSuccessSelectedRegions, out ho_DownSuccessSelectedRegions1,
                    "area", "and", 10000, 158720);
                HOperatorSet.AreaCenter(ho_DownSuccessSelectedRegions1, out hv_DownSuccessArea,
                    out hv_DownSuccessRow, out hv_DownSuccessColumn);

                ho_DownFailRegions.Dispose();
                HOperatorSet.Threshold(ho_DownImageReduced, out ho_DownFailRegions, 138, 140);
                ho_DownFailConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_DownFailRegions, out ho_DownFailConnectedRegions
                    );
                ho_DownFailSelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_DownFailConnectedRegions, out ho_DownFailSelectedRegions,
                    "rectangularity", "and", 0.8, 1);
                ho_DownFailSelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_DownFailSelectedRegions, out ho_DownFailSelectedRegions1,
                    "area", "and", 10000, 158720);
                HOperatorSet.AreaCenter(ho_DownFailSelectedRegions1, out hv_DownFailArea, out hv_DownFailRow,
                    out hv_DownFailColumn);

                if ((int)((new HTuple((new HTuple(hv_UpTestScore.TupleLength())).TupleEqual(
                    0))).TupleOr(new HTuple(((hv_UpTestScore.TupleSelect(0))).TupleLess(0.75)))) != 0)
                {

                    if ((int)((new HTuple((new HTuple(hv_upStartStateArea.TupleLength())).TupleEqual(
                        0))).TupleOr(new HTuple(hv_upStartStateArea.TupleEqual(0)))) != 0)
                    {
                        //��ʼ״̬
                        hv_outport_upresult = 0;
                    }
                    else
                    {
                        //������
                        hv_outport_upresult = 1;
                    }

                }
                else
                {

                    if ((int)(new HTuple((new HTuple(hv_UpSuccessArea.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        //�Ͽ���Գɹ�
                        hv_outport_upresult = 2;
                    }
                    else if ((int)(new HTuple((new HTuple(hv_UpFailArea.TupleLength()
                        )).TupleGreater(0))) != 0)
                    {
                        //�Ͽ����ʧ��
                        hv_outport_upresult = 3;
                    }
                    else
                    {
                        //�ҵ����Խ�����ʶ����֪���ǲ��Գɹ����ǲ���ʧ��
                        hv_outport_upresult = 4;
                    }
                }

                if ((int)((new HTuple((new HTuple(hv_DownTestScore.TupleLength())).TupleEqual(
                    0))).TupleOr(new HTuple(((hv_DownTestScore.TupleSelect(0))).TupleLess(0.75)))) != 0)
                {
                    //������

                    if ((int)((new HTuple((new HTuple(hv_downStartStateArea.TupleLength())).TupleEqual(
                        0))).TupleOr(new HTuple(hv_downStartStateArea.TupleEqual(0)))) != 0)
                    {
                        //��ʼ״̬
                        hv_outport_downresult = 0;
                    }
                    else
                    {
                        //������
                        hv_outport_downresult = 1;
                    }

                }
                else
                {

                    if ((int)(new HTuple((new HTuple(hv_DownSuccessArea.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        //�¿���Գɹ�
                        hv_outport_downresult = 2;
                    }
                    else if ((int)(new HTuple((new HTuple(hv_DownFailArea.TupleLength()
                        )).TupleGreater(0))) != 0)
                    {
                        //�¿����ʧ��
                        hv_outport_downresult = 3;
                    }
                    else
                    {
                        //�ҵ����Խ�����ʶ����֪���ǲ��Գɹ����ǲ���ʧ��
                        hv_outport_downresult = 4;
                    }
                }
                hv_outport_result[0] = hv_outport_upresult.TupleSelect(0);
                hv_outport_result[1] = hv_outport_downresult.TupleSelect(0);
                HOperatorSet.ClearNccModel(hv_TestModelID);
            }
            HOperatorSet.ClearNccModel(hv_RightModelID);
            ho_UpRectangle.Dispose();
            ho_UpImageReduced.Dispose();
            ho_DownRectangle.Dispose();
            ho_DownImageReduced.Dispose();
            ho_UpSuccessRegions.Dispose();
            ho_UpSuccessConnectedRegions.Dispose();
            ho_UpSuccessSelectedRegions.Dispose();
            ho_UpSuccessSelectedRegions1.Dispose();
            ho_UpFailRegions.Dispose();
            ho_UpFailConnectedRegions.Dispose();
            ho_UpFailSelectedRegions.Dispose();
            ho_UpFailSelectedRegions1.Dispose();
            ho_DownSuccessRegions.Dispose();
            ho_DownSuccessConnectedRegions.Dispose();
            ho_DownSuccessSelectedRegions.Dispose();
            ho_DownSuccessSelectedRegions1.Dispose();
            ho_DownFailRegions.Dispose();
            ho_DownFailConnectedRegions.Dispose();
            ho_DownFailSelectedRegions.Dispose();
            ho_DownFailSelectedRegions1.Dispose();

            return;
        }

        // Main procedure 
        private void action()
        {

            // Local iconic variables 


            // Local control variables 

            // Initialize local and output iconic variables 



            //get_image_from_file (Image, 'C:/Users/shen/Desktop/model/success.bmp')
            //do_inspect1 (Image, 'C:/Users/shen/Desktop/model/rightResult.ncm', 'C:/Users/shen/Desktop/model/testover.ncm', outport_result)




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

