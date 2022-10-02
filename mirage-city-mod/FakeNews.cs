using ICities;

namespace mirage_city_mod
{

    class FakeNews : ThreadingExtensionBase
    {

        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();

            // everyone is a genius
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.EducationElementary, 10000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.EducationHighSchool, 10000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.EducationUniversity, 10000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.EducationLibrary, 10000);

            District city = DistrictManager.instance.m_districts.m_buffer[0];
            city.m_productionData.m_tempEducation1Capacity += 1000000u;
            city.m_productionData.m_tempEducation2Capacity += 1000000u;
            city.m_productionData.m_tempEducation3Capacity += 1000000u;

        }
    }

}