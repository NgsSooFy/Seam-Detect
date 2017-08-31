using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamDetect
{
    class ProfileData
    {
        public List<int> OriginProfileData;

        private int pointCount;
        private List<double> ProfileData_float;


        public ProfileData(List<int> profile)
        {
            OriginProfileData = profile;
            pointCount = OriginProfileData.Count;
        }

        

    }
}
