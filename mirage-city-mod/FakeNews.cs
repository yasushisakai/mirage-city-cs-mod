using System;
using ColossalFramework;
using ICities;
using UnityEngine;

// based on this 
// https://github.com/Mbyron26/GameAnarchy/blob/master/Patches/CityServicesPatch.cs
// all credits goes to github user Mbyron26

namespace mirage_city_mod
{
    public class FakeNews : ThreadingExtensionBase
    {
        public static FakeNews instance { get; protected set; }
        //public static BuildingInfo
        public Action[] handers;
        public FakeNews()
        {
            instance = this;
            handers = new Action[] {
                RemoveNoisePollution,
                RemoveGroundPollution,
                RemoveWaterPollution,
                RemoveDeath,
                RemoveGarbage,
                RemoveCrime,
                RemoveFire,
                MaximizeAttractiveness,
                MaximizeEntertainment,
                MaximizeLandValue,
                MaximizeEducationCoverage,
                RemoveBuildingProblem
            };
        }
        public uint m_frameIndex;
        public byte m_frame_16;
        public static ushort numCitizenUnitChunks = 32;
        public static ushort numBuildingChunks = 32;
        public ushort m_nextCitizenUnitChunk;
        public ushort m_nextBuildingChunk;
        public static bool InGame
        {
            get
            {
                var loading = instance?.managers.loading;
                if (loading == null) return false;
                return loading.loadingComplete && (loading.currentMode == AppMode.Game);
            }
        }

        public override void OnAfterSimulationFrame()
        {
            if (!InGame) return;
            if (SimulationManager.instance.SimulationPaused) return;
            m_frameIndex = SimulationManager.instance.m_currentFrameIndex;
            m_frame_16 = (byte)(m_frameIndex & 15);
            var handlerIndex = m_frame_16;
            if (handers.Length <= handlerIndex) return;
            Action handler = handers[handlerIndex];
            handler();
        }

        private void RemoveNoisePollution()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, -100000);
        }
        private void RemoveGroundPollution()
        {
            Singleton<NaturalResourceManager>.instance.AddPollutionDisposeRate(10000);
        }
        private void RemoveWaterPollution()
        {
            Singleton<TerrainManager>.instance.WaterSimulation.AddPollutionDisposeRate(10000);
        }
        private void RemoveDeath()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.DeathCare, 100000);
        }
        private void RemoveGarbage()
        {
        }
        private void RemoveCrime()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.CrimeRate, -100000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.PoliceDepartment, 100000);
        }
        private void RemoveFire()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.FireHazard, -100000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.FireDepartment, 100000);
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.FirewatchCoverage, 100000, Vector3.zero, 100000);
        }

        private void MaximizeAttractiveness()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, 100000);
        }
        private void MaximizeEntertainment()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, 100000);
        }
        private void MaximizeLandValue()
        {
            ImmaterialResourceManager.instance.AddResource(ImmaterialResourceManager.Resource.LandValue, 100000);
        }
        private void MaximizeEducationCoverage()
        {
            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationUniversity, 100000);
            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationHighSchool, 100000);
            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationElementary, 100000);
            DistrictManager dManager = Singleton<DistrictManager>.instance;
            District city = dManager.m_districts.m_buffer[0];
            city.m_productionData.m_tempEducation1Capacity += 1000000u;
            city.m_productionData.m_tempEducation2Capacity += 1000000u;
            city.m_productionData.m_tempEducation3Capacity += 1000000u;
        }
        private void RemoveBuildingProblem()
        {
            BuildingManager bManager = Singleton<BuildingManager>.instance;
            var bBuffer = bManager.m_buildings.m_buffer;
            var chunkSize = bBuffer.Length / numBuildingChunks;
            var begin = (uint)(m_nextBuildingChunk * chunkSize);
            var end = (uint)Math.Min(begin + chunkSize, bBuffer.Length);
            m_nextBuildingChunk += 1;
            m_nextBuildingChunk %= numBuildingChunks;
            for (uint i = begin; i < end; i++)
            {
                if ((bBuffer[i].m_flags & Building.Flags.Created) == Building.Flags.None) continue;
                RemoveBuildingProblemBase(ref bBuffer[i]);
            }
        }

        private void RemoveBuildingProblemBase(ref Building building)
        {
            {
                CitizenManager cManager = Singleton<CitizenManager>.instance;
                var mBuffer = cManager.m_units.m_buffer;
                for (var cID = building.m_citizenUnits; cID != 0; cID = mBuffer[cID].m_nextUnit)
                {
                    GetCitizenUnitDeath(cID, ref mBuffer[cID]);
                }
            }
        }

        private void GetCitizenUnitDeath(uint cuID, ref CitizenUnit citizen)
        {
            CitizenManager cManager = Singleton<CitizenManager>.instance;
            var mBuffer = cManager.m_citizens.m_buffer;
            var ciBuffer = cManager.m_instances.m_buffer;
            for (int i = 0; i < 5; i++)
            {
                uint citizenID = citizen.GetCitizen(i);
                if (citizenID != 0u)
                {
                    if (mBuffer[citizenID].Dead)
                    {
                        ushort citizenInstanceId = mBuffer[citizenID].m_instance;
                        if (citizenInstanceId != 0)
                        {
                            mBuffer[citizenID].m_instance = 0;
                            ciBuffer[citizenInstanceId].m_citizen = 0u;
                            cManager.ReleaseCitizenInstance(citizenInstanceId);
                        }
                        var family = mBuffer[citizenID].m_family;
                        cManager.ReleaseCitizen(citizenID);
                        cManager.CreateCitizen(out citizenID, 5, family, ref SimulationManager.instance.m_randomizer);
                    }
                }
            }
        }

    }
}
