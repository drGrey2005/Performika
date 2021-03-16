using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Npgsql;
using NpgsqlTypes;
using PerformikaLib.Entities;

namespace PerformikaDb
{
    /// <summary>
    /// Класс-загрузчик данных, полученных из Перформики в базу данных PostgreSQL
    /// </summary>
    public class DbLoader
    {
        private readonly string _connectionString;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public DbLoader(string connectionString) => _connectionString = connectionString;

        public void LoadProgramTypesInDb(List<ProgramInfo> programInfos)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_program(@program_uid,@program_type_id,@program_year,@fku_id,@status,@change_date)",
                        conn);

                com.Parameters.Add("program_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("program_type_id", NpgsqlDbType.Integer);
                com.Parameters.Add("program_year", NpgsqlDbType.Integer);
                com.Parameters.Add("fku_id", NpgsqlDbType.Integer);
                com.Parameters.Add("status", NpgsqlDbType.Varchar);
                com.Parameters.Add("change_date", NpgsqlDbType.Timestamp);


                foreach (ProgramInfo info in programInfos)
                {
                    com.Parameters["@program_uid"].Value = info.Id;
                    com.Parameters["@program_type_id"].Value = info.ProgramType;
                    com.Parameters["@program_year"].Value = info.Year;
                    com.Parameters["@fku_id"].Value = info.Fku;
                    com.Parameters["@status"].Value = info.Status;
                    com.Parameters["@change_date"].Value = info.ChangeDate;

                    com.ExecuteNonQuery();
                }
            }
        }
        public void LoadProgramObjectsInDb(List<ProgramObjectInfo> objectInfos)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_program_object(@object_uid,@object_name,@region_id,@status,@road_section_id,@nrs_id,@object_type,@isso_uid,@isso_name,@start_value,@start_add_value,@year_start,@year_end,@change_date)",
                        conn);

                com.Parameters.Add("object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("object_name", NpgsqlDbType.Varchar);
                com.Parameters.Add("region_id", NpgsqlDbType.Integer);
                com.Parameters.Add("status", NpgsqlDbType.Varchar);
                com.Parameters.Add("road_section_id", NpgsqlDbType.Integer);
                com.Parameters.Add("nrs_id", NpgsqlDbType.Integer);
                com.Parameters.Add("object_type", NpgsqlDbType.Integer);


                com.Parameters.Add("isso_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("isso_name", NpgsqlDbType.Varchar);
                com.Parameters.Add("start_value", NpgsqlDbType.Integer);
                com.Parameters.Add("start_add_value", NpgsqlDbType.Integer);


                com.Parameters.Add("year_start", NpgsqlDbType.Integer);
                com.Parameters.Add("year_end", NpgsqlDbType.Integer);
                com.Parameters.Add("change_date", NpgsqlDbType.Timestamp);


                foreach (ProgramObjectInfo info in objectInfos)
                {
                    com.Parameters["@object_uid"].Value = info.Id;
                    com.Parameters["@object_name"].Value = info.ObjectName;
                    com.Parameters["@region_id"].Value = info.RegionId;
                    com.Parameters["@status"].Value = info.Status;

                    if (info.RoadSectionId == null)
                    {
                        com.Parameters["@road_section_id"].Value = DBNull.Value;
                    }
                    else
                    {
                        com.Parameters["@road_section_id"].Value = info.RoadSectionId;

                    }

                    if (info.NrsId == null)
                    {
                        com.Parameters["@nrs_id"].Value = DBNull.Value;
                    }
                    else
                    {
                        com.Parameters["@nrs_id"].Value = info.NrsId;
                    }

                    if (info.Type == null)
                    {
                        com.Parameters["@object_type"].Value = DBNull.Value;
                    }
                    else
                    {
                        com.Parameters["@object_type"].Value = info.Type;

                    }

                    if (info.IssoUid == null)
                    {
                        com.Parameters["@isso_uid"].Value = DBNull.Value;
                    }
                    else
                    {
                        com.Parameters["@isso_uid"].Value = info.IssoUid;
                    }

                    com.Parameters["@isso_name"].Value = DBNull.Value;
                    com.Parameters["@start_value"].Value = DBNull.Value;
                    com.Parameters["@start_add_value"].Value = DBNull.Value;




                    com.Parameters["@year_start"].Value = info.YearStart;
                    com.Parameters["@year_end"].Value = info.YearEnd;
                    com.Parameters["@change_date"].Value = info.ChangeDate;

                    com.ExecuteNonQuery();
                }
            }
        }
        public void LoadProgramObjectsConnection(HashSet<(Guid UID, int programType, int startYear, int endYear, int fku)> values)
        {

            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_program_object_connection(@object_uid,@program_type_id,@fku_id,@year_start,@year_end)",
                        conn);

                com.Parameters.Add("object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("program_type_id", NpgsqlDbType.Integer);
                com.Parameters.Add("fku_id", NpgsqlDbType.Integer);
                com.Parameters.Add("year_start", NpgsqlDbType.Integer);
                com.Parameters.Add("year_end", NpgsqlDbType.Integer);

                foreach ((Guid UID, int programType, int startYear, int endYear, int fku) info in values)
                {
                    com.Parameters["@object_uid"].Value = info.UID;
                    com.Parameters["@program_type_id"].Value = info.programType;
                    com.Parameters["@fku_id"].Value = info.fku;
                    com.Parameters["@year_start"].Value = info.startYear;
                    com.Parameters["@year_end"].Value = info.endYear;
                    com.ExecuteNonQuery();
                }
            }
        }
        public void LoadChildRoadsInDb(List<RoadRepair> roadRepairs)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_child_road(@child_object_uid,@road_section_id,@start_value,@start_add_value,@finish_value,@finish_add_value,@object_id,@object_uid,@change_date)",
                        conn);

                com.Parameters.Add("child_object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("road_section_id", NpgsqlDbType.Integer);
                com.Parameters.Add("start_value", NpgsqlDbType.Integer);
                com.Parameters.Add("start_add_value", NpgsqlDbType.Integer);
                com.Parameters.Add("finish_value", NpgsqlDbType.Integer);
                com.Parameters.Add("finish_add_value", NpgsqlDbType.Integer);
                com.Parameters.Add("object_id", NpgsqlDbType.Integer);
                com.Parameters.Add("object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("change_date", NpgsqlDbType.Timestamp);


                dynamic GetId(int? id)
                {
                    if (id == null)
                    {
                        return DBNull.Value;
                    }

                    return id;
                }

                foreach (RoadRepair info in roadRepairs)
                {
                    com.Parameters["@child_object_uid"].Value = info.ChildObjectUid;
                    com.Parameters["@road_section_id"].Value = GetId(info.RoadSectionId);
                    com.Parameters["@start_value"].Value = info.Start;
                    com.Parameters["@start_add_value"].Value = info.StartAdd;
                    com.Parameters["@finish_value"].Value = info.Finish;
                    com.Parameters["@finish_add_value"].Value = info.FinishAdd;
                    com.Parameters["@object_id"].Value = GetId(info.ObjectId);
                    com.Parameters["@object_uid"].Value = info.ObjectUid;
                    com.Parameters["@change_date"].Value = info.ChangeDate;

                    try
                    {
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public void LoadChildRoadsIssoInDb(List<RoadRepairIsso> roadRepairIssos)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_child_isso(@child_object_uid,@road_section_id,@start_value,@start_add_value,@isso_name,@isso_uid,@abdm_id,@object_id,@object_uid,@change_date)",
                        conn);

                com.Parameters.Add("child_object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("road_section_id", NpgsqlDbType.Integer);
                com.Parameters.Add("start_value", NpgsqlDbType.Integer);
                com.Parameters.Add("start_add_value", NpgsqlDbType.Integer);
                com.Parameters.Add("isso_name", NpgsqlDbType.Varchar);
                com.Parameters.Add("isso_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("abdm_id", NpgsqlDbType.Integer);
                com.Parameters.Add("object_id", NpgsqlDbType.Integer);
                com.Parameters.Add("object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("change_date", NpgsqlDbType.Timestamp);


                dynamic GetId(int? id)
                {
                    if (id == null)
                    {
                        return DBNull.Value;
                    }

                    return id;
                }

                foreach (RoadRepairIsso info in roadRepairIssos)
                {
                    com.Parameters["@child_object_uid"].Value = info.ChildObjectUid;
                    com.Parameters["@road_section_id"].Value = GetId(info.RoadSectionId);
                    com.Parameters["@start_value"].Value = DBNull.Value; //info.Start;
                    com.Parameters["@start_add_value"].Value = DBNull.Value; //info.StartAdd;
                    com.Parameters["@isso_name"].Value = DBNull.Value; //info.Name;

                    if (info.IssoUid == null)
                    {
                        com.Parameters["@isso_uid"].Value = DBNull.Value;
                    }
                    else
                    {
                        com.Parameters["@isso_uid"].Value = info.IssoUid;

                    }

                    com.Parameters["@abdm_id"].Value = GetId(info.AbdmId);
                    com.Parameters["@object_id"].Value = GetId(info.ObjectId);
                    com.Parameters["@object_uid"].Value = info.ObjectUid;
                    com.Parameters["@change_date"].Value = info.ChangeDate;

                    try
                    {
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public void LoadIssoDictInDb(List<IssoValue> issoValues)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        "CALL public.prc_insert_isso(@object_uid,@id_abdm,@road,@road_uid,@name_value,@description,@type_value,@type_uid,@region,@region_uid,@category,@length_value,@evalution,@road_size,@build_recon_year,@capacity,@diagnostics_year,@fku,@fku_uid,@change_date)",
                        conn);

                com.Parameters.Add("object_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("id_abdm", NpgsqlDbType.Integer);
                com.Parameters.Add("road", NpgsqlDbType.Varchar);
                com.Parameters.Add("road_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("name_value", NpgsqlDbType.Varchar);
                com.Parameters.Add("description", NpgsqlDbType.Varchar);
                com.Parameters.Add("type_value", NpgsqlDbType.Varchar);
                com.Parameters.Add("type_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("region", NpgsqlDbType.Varchar);
                com.Parameters.Add("region_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("category", NpgsqlDbType.Varchar);
                com.Parameters.Add("length_value", NpgsqlDbType.Integer);
                com.Parameters.Add("evalution", NpgsqlDbType.Varchar);
                com.Parameters.Add("road_size", NpgsqlDbType.Integer);
                com.Parameters.Add("build_recon_year", NpgsqlDbType.Varchar);
                com.Parameters.Add("capacity", NpgsqlDbType.Integer);
                com.Parameters.Add("diagnostics_year", NpgsqlDbType.Date);
                com.Parameters.Add("fku", NpgsqlDbType.Varchar);
                com.Parameters.Add("fku_uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("change_date", NpgsqlDbType.Timestamp);


                dynamic GetValue(dynamic val)
                {
                    if (val == null)
                    {
                        return DBNull.Value;
                    }

                    return val;
                }


                foreach (IssoValue info in issoValues)
                {

                    com.Parameters["@object_uid"].Value = info.ObjectUid;
                    com.Parameters["@id_abdm"].Value = GetValue(info.AbdmId);
                    com.Parameters["@road"].Value = GetValue(info.Road);
                    com.Parameters["@road_uid"].Value = GetValue(info.RoadUid);
                    com.Parameters["@name_value"].Value = GetValue(info.Name);
                    com.Parameters["@description"].Value = GetValue(info.Description);
                    com.Parameters["@type_value"].Value = GetValue(info.Type);
                    com.Parameters["@type_uid"].Value = GetValue(info.TypeUid);
                    com.Parameters["@region"].Value = GetValue(info.Region);
                    com.Parameters["@region_uid"].Value = GetValue(info.RegionUid);
                    com.Parameters["@category"].Value = GetValue(info.Category);
                    com.Parameters["@length_value"].Value = GetValue(info.Length);
                    com.Parameters["@evalution"].Value = GetValue(info.Evaluation);
                    com.Parameters["@road_size"].Value = GetValue(info.RoadSize);
                    com.Parameters["@build_recon_year"].Value = GetValue(info.BuildReconYear);
                    com.Parameters["@capacity"].Value = GetValue(info.Capacity);
                    com.Parameters["@diagnostics_year"].Value = GetValue(info.DiagnosticsYear);
                    com.Parameters["@fku"].Value = GetValue(info.Fku);
                    com.Parameters["@fku_uid"].Value = GetValue(info.FkuUid);
                    com.Parameters["@change_date"].Value = GetValue(info.ChangeDate);

                    try
                    {
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


        public void UpdateRoadSection(Dictionary<string, string> roadSections)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        $@"UPDATE dic.""RoadSection"" SET ""Uid"" = @uid WHERE ""Id"" = @id",
                        conn);

                com.Parameters.Add("uid", NpgsqlDbType.Uuid);
                com.Parameters.Add("id", NpgsqlDbType.Integer);

                foreach (KeyValuePair<string, string> roadSection in roadSections)
                {
                    com.Parameters["@uid"].Value = Guid.Parse(roadSection.Key);
                    com.Parameters["@id"].Value = int.Parse(roadSection.Value);

                    try
                    {
                        com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        //Получение списка всех идентификаторов из таблицы БД
        private List<Guid> GetUids(string tableName, string columnName)
        {
            List<Guid> guids = new List<Guid>();
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();

                NpgsqlCommand com =
                    new NpgsqlCommand(
                        $@"SELECT {columnName} FROM {tableName}",
                        conn);
                NpgsqlDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    guids.Add(reader.GetGuid(0));
                }

            }

            return guids;
        }

        /// <summary>
        /// Удаление записей из таблицы по переданным идентификаторам
        /// </summary>
        /// <param name="tableName">Имя таблицы для обработки</param>
        /// <param name="columnName">Название столбца с идентификаторами</param>
        /// <param name="performikaUids">Значения для удаления</param>
        /// <returns>Число удаленных записей</returns>
        public int DeleteRows(string tableName, string columnName, List<Guid> performikaUids)
        {
            List<Guid> postgresUids = GetUids(tableName, columnName);
            _logger.Info($"Выявлены данные в таблице {tableName} для удаления: {string.Join(", ", postgresUids.Except(performikaUids).Select(val => $"'{val}'"))}");
            using (NpgsqlConnection conn = GetConnection())
            {
                NpgsqlCommand com =
                    new NpgsqlCommand(
                        $@"DELETE FROM {tableName} WHERE {columnName} IN ({string.Join(", ", postgresUids.Except(performikaUids).Select(val => $"'{val}'"))})",
                        conn);
                conn.Open();
                int count = com.ExecuteNonQuery();
                return count;
            }
        }
    }
}