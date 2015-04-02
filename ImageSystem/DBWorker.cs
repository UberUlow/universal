using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace UniversalSystem
{
    public class DbWorker
    {
        private SqlConnection connection;
        private SqlCommand command;
        private SqlParameter parametr;
        private SqlDataReader reader;

        /// <summary>
        /// Соединение с БД
        /// </summary>
        /// <param name="srvname">Имя сервера</param>
        /// <returns></returns>
        public bool ConnecToDb(string srvname)
        {
            connection = new SqlConnection(@"Data Source=" + srvname + @";Initial Catalog=ProjectX;Integrated Security=True");
            try
            {
                connection.Open();
            }
            catch (SqlException)
            {
                return false;
            }
            return true;
        }

        public bool IsConnected
        {
            get { return connection.State == ConnectionState.Open; }
        }

        /// <summary>
        /// Добавление вида в БД
        /// </summary>
        /// <param name="nameKind">Имя вида</param>
        public void AddKind(string nameKind)
        {
            command = new SqlCommand(@"INSERT INTO Kinds (Name) values(@nameKind)", connection);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind};
            command.Parameters.Add(parametr);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Добавление сорта в БД
        /// </summary>
        /// <param name="nameSort">Имя сорта</param>
        /// <param name="nameKind">Имя вида</param>
        public void AddSort(string nameSort, string nameKind)
        {
            command = new SqlCommand(@"INSERT INTO Sorts(Name, ID_Kinds) values(@nameSort,(Select ID FROM Kinds WHERE Name=@nameKind))", connection);
            parametr = new SqlParameter("@nameSort", SqlDbType.NChar) { Value = nameSort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind };
            command.Parameters.Add(parametr);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Добавление изображения в БД
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="nameKind">Имя вида</param>
        /// <param name="nameSort">Имя сорта</param>
        public void AddImage(Bitmap image, string nameKind, string nameSort)
        {
            command = new SqlCommand(@"INSERT INTO Images(Image, ID_Sorts) values(@image,(select id from Sorts where Name = @nameSort and ID_Kinds = (select id from Kinds where Name=@nameKind)))", connection);
            parametr = new SqlParameter("@image", SqlDbType.Image);
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            parametr.Value = ms.ToArray();
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameSort", SqlDbType.NChar) { Value = nameSort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind };
            command.Parameters.Add(parametr);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Добавление дескрипторов
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nameKind">Имя вида</param>
        /// <param name="nameSort">Имя сорта</param>
        public void AddDescriptors(ref Data data, string nameKind, string nameSort)
        {
            command = new SqlCommand(@"INSERT INTO Descriptors(DispR, DispG, DispB, ExpR, ExpG, ExpB, RatioSP,RatioDiag, ID_Sorts) 
            values(@dispR, @dispG, @dispB, @expR, @expG, @expB, @ratioSP,@ratioDiag,(select id from Sorts where Name = @nameSort and ID_Kinds = (select id from Kinds where Name=@nameKind)))", connection);
            parametr = new SqlParameter("@dispR", SqlDbType.Float) { Value = data.Dispersion.R };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@dispG", SqlDbType.Float) { Value = data.Dispersion.G };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@dispB", SqlDbType.Float) { Value = data.Dispersion.B };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@expR", SqlDbType.Float) { Value = data.Expectation.R };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@expG", SqlDbType.Float) { Value = data.Expectation.G };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@expB", SqlDbType.Float) { Value = data.Expectation.B };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@ratioSP", SqlDbType.Float) { Value = data.RatioSP };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@ratioDiag", SqlDbType.Float) { Value = data.RatioDiagonals };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameSort", SqlDbType.NChar) { Value = nameSort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind };
            command.Parameters.Add(parametr);
            command.ExecuteNonQuery();
        }

        #region Чтение данных из БД

        /// <summary>
        /// Чтение видов из БД
        /// </summary>
        public List<string> ReadKinds()
        {
            var kinds = new List<string>();
            command = new SqlCommand("SELECT Name FROM Kinds", connection);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    kinds.Add(reader.GetString(0));
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return kinds;
        }

        /// <summary>
        /// Чтение вида по сорту
        /// </summary>
        /// <param name="nameSort"></param>
        /// <returns></returns>
        public List<string> ReadKinds(string nameSort)
        {
            var kind = new List<string>();
            command = new SqlCommand(@"SELECT Name FROM Sorts WHERE ID_Kinds = (SELECT ID FROM Kinds WHERE Name = @nameSort)", connection);
            parametr = new SqlParameter("@nameSort", SqlDbType.NChar) { Value = nameSort };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    kind.Add(reader.GetString(0));
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return kind;
        }

        /// <summary>
        /// Чтение сортов из БД
        /// </summary>
        /// <remarks>SELECT Name FROM Sorts</remarks>
        public List<string> ReadSorts()
        {
            var sorts = new List<string>();
            command = new SqlCommand("SELECT Name FROM Sorts", connection);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    sorts.Add(reader.GetString(0));
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return sorts;
        }

        /// <summary>
        /// Чтение сортов
        /// </summary>
        /// <param name="nameKind">Имя вида</param>
        public List<string> ReadSorts(string nameKind)
        {
            var sorts = new List<string>();
            command = new SqlCommand(@"SELECT Name FROM Sorts WHERE ID_Kinds = (SELECT ID FROM Kinds WHERE Name = @nameKind)", connection);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    sorts.Add(reader.GetString(0));
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return sorts;
        }

        /// <summary>
        /// Чтение изображения из БД по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Bitmap ReadImage(int id)
        {
            command = new SqlCommand("SELECT Image FROM Images WHERE Images.ID = @ID", connection);
            parametr = new SqlParameter("@ID", SqlDbType.Int) { Value = id };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                var tempBytes = new byte[52428800];
                int bLength = (int)reader.GetBytes(0, 0, tempBytes, 0, tempBytes.Length);
                var array = new byte[bLength];
                Array.Copy(tempBytes, array, bLength);
                var image = new Bitmap(Image.FromStream(new MemoryStream(array)));
                reader.Close();
                return image;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// Чтение изображения из БД по сорту
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public List<Bitmap> ReadImages(string kind, string sort)
        {
            var images = new List<Bitmap>();
            command = new SqlCommand("SELECT Image FROM Images WHERE ID_Sorts = (SELECT ID FROM Sorts WHERE Name = @sort AND ID_Kinds=(select ID from Kinds where Name = @kind))", connection);
            parametr = new SqlParameter("@sort", SqlDbType.NChar) { Value = sort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@kind", SqlDbType.NChar) { Value = kind };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while ( reader.Read())
                {
                    var tempBytes = new byte[52428800];
                    var bLength = (int)reader.GetBytes(0, 0, tempBytes, 0, tempBytes.Length);
                    var array = new byte[bLength];
                    Array.Copy(tempBytes, array, bLength);
                    images.Add(new Bitmap(Image.FromStream(new MemoryStream(array))));
                }
                reader.Close();
                return images;
            }
            reader.Close();
            return null;
        }

        public Bitmap ReadImage(string kind, string sort)
        {
            command = new SqlCommand("SELECT Image FROM Images WHERE ID_Sorts = (SELECT ID FROM Sorts WHERE Name = @sort AND ID_Kinds=(select ID from Kinds where Name = @kind))", connection);
            parametr = new SqlParameter("@sort", SqlDbType.NChar) { Value = sort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@kind", SqlDbType.NChar) { Value = kind };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                var tempBytes = new byte[52428800];
                var bLength = (int) reader.GetBytes(0, 0, tempBytes, 0, tempBytes.Length);
                var array = new byte[bLength];
                Array.Copy(tempBytes, array, bLength);
                var image = new Bitmap(Image.FromStream(new MemoryStream(array)));
                reader.Close();
                return image;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// Чтение дескрипторов по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Data> ReadData(int id)
        {
            var descriptors = new List<Data>();
            command = new SqlCommand(@"SELECT * FROM Descriptors WHERE ID = @ID", connection);
            parametr = new SqlParameter("@ID", SqlDbType.Int) { Value = id };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Data data = new Data();
                    data.Id = reader.GetInt32(0);
                    data.Dispersion.R = reader.GetDouble(2);
                    data.Dispersion.G = reader.GetDouble(3);
                    data.Dispersion.B = reader.GetDouble(4);
                    data.Expectation.R = reader.GetDouble(5);
                    data.Expectation.G = reader.GetDouble(6);
                    data.Expectation.B = reader.GetDouble(7);
                    data.RatioSP = reader.GetDouble(8);
                    data.RatioDiagonals = reader.GetDouble(9);
                    descriptors.Add(data);
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return descriptors;
        }

        /// <summary>
        /// Чтение дескрипторов по сорту
        /// </summary>
        /// <param name="nameSort">Имя сорта</param>
        /// <param name="nameKind">Имя вида</param>
        /// <returns></returns>
        public List<Data> ReadDescriptors(string nameSort, string nameKind)
        {
            var descriptors = new List<Data>();
            command = new SqlCommand(@"SELECT * FROM Descriptors WHERE ID_Sorts = (SELECT ID FROM Sorts WHERE Name = @nameSort and ID_Kinds=(select id from Kinds where Name = @nameKind))", connection);
            parametr = new SqlParameter("@nameSort", SqlDbType.NChar) { Value = nameSort };
            command.Parameters.Add(parametr);
            parametr = new SqlParameter("@nameKind", SqlDbType.NChar) { Value = nameKind };
            command.Parameters.Add(parametr);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Data data = new Data();
                    data.Id = reader.GetInt32(0);
                    data.Dispersion.R = reader.GetDouble(1);
                    data.Dispersion.G = reader.GetDouble(2);
                    data.Dispersion.B = reader.GetDouble(3);
                    data.Expectation.R = reader.GetDouble(4);
                    data.Expectation.G = reader.GetDouble(5);
                    data.Expectation.B = reader.GetDouble(6);
                    data.RatioSP = reader.GetDouble(7);
                    data.RatioDiagonals = reader.GetDouble(8);
                    descriptors.Add(data);
                }
            }
            else
            {
                reader.Close();
                return null;
            }
            reader.Close();
            return descriptors;
        }

        #endregion
    }
}
