using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.SqlServer;
using System.Globalization;

namespace MLSMarketing
{
    public class dbHelper
    {
        public List<CodigoPostal> SelecioneCodigopostal(string StrSql)
        {
            try
            {
                List<CodigoPostal> codigoPostals = new List<CodigoPostal>();
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlCommand = new SqlCommand();
                da.SelectCommand = sqlCommand;
                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
                da.SelectCommand.Connection = sqlConn;

                da.SelectCommand.CommandType = CommandType.Text;
                da.SelectCommand.CommandText = StrSql;
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    CodigoPostal codigoPostal = new CodigoPostal()
                    {
                        CodigoPostalRaiz = row["CodigoPostalRaiz"].ToString(),
                        Concelho = row["Concelho"].ToString(),
                        Distrito = row["Distrito"].ToString(),
                        Freguesia = row["Freguesia"].ToString(),
                        Id = Convert.ToInt32(row["Id"].ToString()),
                        Rua = row["Rua"].ToString()
                    };
                    codigoPostals.Add(codigoPostal);
                }

                return codigoPostals;

            }
            catch (Exception ex)
            {
                return new List<CodigoPostal>();
            }

        }
        public List<Imoveis> SelecionePesq(string StrSql)
        {
            try
            {
                List<Imoveis> imoveis = new List<Imoveis>();
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlCommand = new SqlCommand();
                da.SelectCommand = sqlCommand;
                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
                da.SelectCommand.Connection = sqlConn;

                da.SelectCommand.CommandType = CommandType.Text;
                da.SelectCommand.CommandText = StrSql;
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    Imoveis mls = new Imoveis()
                    {
                        Id_Imovel = Convert.ToInt32(row["Id_Imovel"].ToString()),
                        property_area = Convert.ToInt32(row["property_area"].ToString()),
                        property_bathrooms = Convert.ToInt32(row["property_bathrooms"].ToString()),
                        property_concelho = row["property_concelho"].ToString(),
                        property_parking = Convert.ToInt32(row["property_parking"].ToString()),
                        LinkImagemPrincipal = row["LinkImagemPrincipal"].ToString(),
                        LinkAnuncio = row["LinkAnuncio"].ToString(),
                        property_bedrooms = Convert.ToInt32(row["property_bedrooms"].ToString()),
                        property_type = row["property_type"].ToString(),
                        property_title = row["property_title"].ToString(),
                        property_price = Convert.ToDecimal(row["property_price"].ToString())
                    };
                    imoveis.Add(mls);
                }

                return imoveis;

            }
            catch (Exception)
            {
                return new List<Imoveis>();
            }

        }

        public List<Freguesia> SelecioneFreguesia(string StrSql)
        {
            try
            {
                List<Freguesia> freguesia = new List<Freguesia>();
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlCommand = new SqlCommand();
                da.SelectCommand = sqlCommand;
                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
                da.SelectCommand.Connection = sqlConn;

                da.SelectCommand.CommandType = CommandType.Text;
                da.SelectCommand.CommandText = StrSql;
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    Freguesia mls = new Freguesia()
                    {
                        Designacao = row["Designacao"].ToString(),
                        DesignacaoCC = row["DesignacaoCC"].ToString(),
                        DesignacaoFR = row["DesignacaoFR"].ToString(),
                        Concelho = row["Concelho"].ToString(),
                        Distrito = row["Distrito"].ToString(),
                        FreguesiaCod = row["FreguesiaCod"].ToString(),
                        Id_Freguesia = Convert.ToInt32(row["Id_Freguesia"].ToString()),
                        Rural = row["Rural"].ToString()
                    };
                    freguesia.Add(mls);
                }

                return freguesia;

            }
            catch (Exception ex)
            {
                return new List<Freguesia>();
            }

        }

        public string ExecutaQuery(string StrSql)
        {
            try
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand sqlCommand = new SqlCommand();
                da.UpdateCommand = sqlCommand;
                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString(); 
                da.UpdateCommand.Connection = sqlConn;

                da.UpdateCommand.CommandType = CommandType.Text;
                da.UpdateCommand.CommandText = StrSql;
                sqlConn.Open();
                da.UpdateCommand.ExecuteNonQuery();
                sqlConn.Close();

                return "Comando Concluido com Sucesso !!";

            }
            catch (Exception ex)
            {

                return "Erro: " + ex.Message;
            }
        }

        public int ExecutaEscalar(string StrSql)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ToString()))
                {
                    int newID;
                    StrSql += ";SELECT CAST(scope_identity() AS int)";

                    using (SqlCommand cmds = new SqlCommand(StrSql, con))
                    {
                        con.Open();
                        newID = (int)cmds.ExecuteScalar();

                        if (con.State == ConnectionState.Open) con.Close();
                        return newID;
                    }
                }
            }
            catch (Exception ex)
            {

                return 0;
            }
        }
    }

}
