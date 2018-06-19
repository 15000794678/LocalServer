using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.IO;

namespace MakeModel
{
    public class HalconBoardTestDll
    {
        private readonly HDevelopExport _halcon = new HDevelopExport();
        private HObject _screemImage = new HObject();
       // List<int> outResult = null;

        private bool ImageFromFile(string imagePath)
        {
            try
            {
                _halcon.get_image_from_file(ref _screemImage, imagePath);
                return true;
            }
            catch
            {
                //_imageLog.WriteLogFile("没有读取到图片");
                return false;
                //throw new Exception(e.Message);
            }
        }

        private bool WriteModel(string imagePath,string modelPath)
        {
            try
            {
                _halcon.write_Model(imagePath, modelPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ReadModel(string modelPath, int modelNum,double minModelScore, out List<int> testResult)
        {
            testResult = null;
            try
            {
                HTuple hvResult;
                _halcon.read_Model(_screemImage, modelPath, modelNum, minModelScore,out hvResult);
                testResult = InsertTupleToList(hvResult);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private List<int> InsertTupleToList(HTuple tuples)
        {
            List<int> lstResult = new List<int>();
            for (int i = 0; i < tuples.Length; i++)
            {
                lstResult.Add(tuples[i].I);
            }
            return lstResult;
        }

        public int Halcon_WriteModel(string imagePath, string modelPath)
        {
            return WriteModel(imagePath,modelPath) ? 1 : 0;
        }



        public int[]  Halcon_ReadModel(string imagePath, string modelPath, int modelNum,double minModelScore)
        {
            
            if (ImageFromFile(imagePath))
            {
                List<int> outResultList = null;
                if (ReadModel(modelPath, modelNum, minModelScore, out outResultList))
                {

                    if (outResultList[0]==0)
                    {
                        //未读到模板
                        int[] outResult = new[] {2};
                        return outResult;
                    }
                    else
                    {
                        int[] outResult = new int[outResultList.Count];
                        for (int i = 0; i < outResultList.Count; i++)
                        {
                            //返回找到模板的坐标 前一半为行坐标，后一半为纵坐标
                            outResult[i] = outResultList[i];
                        }
                        return outResult;
                    }
                }
                else
                {
                    //读模板出现问题
                    int[] outResult = new[] { 1 };
                    return outResult;
                }
            }
            else
            {
                //未读取图片
                int[] outResult = new[] { 0 };
                return outResult;
            }
            
        }


    }
}
