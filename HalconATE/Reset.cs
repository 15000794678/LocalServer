using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HalconATE
{
    public class Reset
    {
        private string testNumModelPath = @"testNum.ncm";
        private string testTimeModelPath = @"testTime.ncm";
        private string lightboxModelPath = @"lightbox.ncm";
        private string resetbuttonModelPath = @"resetbutton.ncm";
        private string cancellightboxModelPath = @"cancellightbox.ncm";
        private string okModelPath = "ok.ncm";
        private string notokModelPath = "notok.ncm";
        private string testingModelPath = "testing.ncm";
        private string startingModelPath = "starting.ncm";


        private string dirPath = string.Empty;

        private HalconBoardTestDll_Reset.HalconBoardTestDll _testDll = new HalconBoardTestDll_Reset.HalconBoardTestDll();
        /*
         * 返回值 ：
         * 000 = 代表三个都齐全；
         * 111 = 代表全没有，
         * 110 = 代表只有box模板；
         * 101 = 代表只有右测试按钮模板；
         * 011 = 代表只有参数完成模板。   
         */
        public int Halcon_CheckModule(string dirpath)
        {
            int result = 0;

            if (!File.Exists(dirpath + testNumModelPath) ||
                !File.Exists(dirpath + testTimeModelPath) ||
                !File.Exists(dirpath + lightboxModelPath) ||
                !File.Exists(dirpath + resetbuttonModelPath) ||
                !File.Exists(dirpath + cancellightboxModelPath) ||
                !File.Exists(dirpath + okModelPath) ||
                !File.Exists(dirpath + notokModelPath) ||
                !File.Exists(dirpath + testingModelPath) ||
                !File.Exists(dirpath + startingModelPath))
            {
                result = 1;
            }

            dirPath = dirpath;
            return result;
        }

        /*
         * 返回值为含三个值得int数组
         * [0,0,0] 未读到图
         * [1,x,y] 有messagebox窗口，x，y为坐标
         * [2,2,2] messagebox窗口未知错误
         * [3,3,3] 未找到定位区域（右测试按钮）
         * [4,4,4] 未知错误
         * [5,a,b] 查询到测试窗体，其中a和b的值只有0,1,2,3,4；
         * 当a=0表示上窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态, 5=暂停状态, 6=测试终止
         * 当b=0表示下窗体初始状态，=1测试中，=2测试成功，=3测试失败，=4未知状态，5=暂停状态，6=测试终止
         */
        public int[] Halcon_CheckImageStatus(string imagePath)
        {
            return _testDll.Halcon_CheckImageStatus(imagePath, 
                                                    dirPath + testNumModelPath, 
                                                    dirPath + testTimeModelPath, 
                                                    dirPath + lightboxModelPath, 
                                                    dirPath + resetbuttonModelPath, 
                                                    dirPath + cancellightboxModelPath, 
                                                    dirPath + okModelPath, 
                                                    dirPath + notokModelPath, 
                                                    dirPath + testingModelPath, 
                                                    dirPath + startingModelPath);
          
        }

        public int Halcon_CheckLightStatus(int b_thresthod, int r_thresthod, int y_thresthod, out int value)
        {
            return _testDll.Halcon_CheckLightStatus(b_thresthod, r_thresthod, y_thresthod, out value);
        }

    }
}
