using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    public class ServicePersistanceAcompleter
    {
        private string connectionString;

        public ServicePersistanceAcompleter(string serverIp, string dbname, string user, string pwd)
        {
            connectionString =
                $"server={serverIp};port=3306;database={dbname};uid={user};pwd={pwd};Connection Timeout=5;";

            using (var conn = OpenConnection()) { }
        }

        public uint SaveGraph(Graph g)
        {
            uint graphId;

            using (var conn = OpenConnection())
            {
                string sqlGraph = @"
                    INSERT INTO Graphe(nb_sommets, est_oriente)
                    VALUES (@n, @oriente);
                    SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sqlGraph, conn))
                {
                    cmd.Parameters.AddWithValue("@n", g.Order);
                    cmd.Parameters.AddWithValue("@oriente", g.estOriente);
                    graphId = Convert.ToUInt32(cmd.ExecuteScalar());
                }

                Dictionary<int, uint> indexToSommetId = new Dictionary<int, uint>();

                foreach (var entree in g.sommetsDico)
                {
                    string nom = entree.Key;
                    int index = entree.Value;
                    float valeur = g.valeursSommets[index];

                    string sqlSommet = @"
                        INSERT INTO Sommet(nom, valeur, graphe_id, ordre)
                        VALUES (@nom, @valeur, @gid, @ordre);
                        SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sqlSommet, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", nom);
                        cmd.Parameters.AddWithValue("@valeur", valeur);
                        cmd.Parameters.AddWithValue("@gid", graphId);
                        cmd.Parameters.AddWithValue("@ordre", index);

                        uint sommetId = Convert.ToUInt32(cmd.ExecuteScalar());
                        indexToSommetId[index] = sommetId;
                    }
                }

                for (int i = 0; i < g.Order; i++)
                {
                    for (int j = 0; j < g.Order; j++)
                    {
                        float poids = g.matAdjacence.GetValue(i, j);

                        if (poids != g.valeurPasDArc)
                        {
                            if (!g.estOriente && j < i)
                                continue;

                            string sqlArc = @"
                                INSERT INTO Arc(source_id, destination_id, poids, graphe_id)
                                VALUES (@source, @dest, @poids, @gid);";

                            using (var cmd = new MySqlCommand(sqlArc, conn))
                            {
                                cmd.Parameters.AddWithValue("@source", indexToSommetId[i]);
                                cmd.Parameters.AddWithValue("@dest", indexToSommetId[j]);
                                cmd.Parameters.AddWithValue("@poids", poids);
                                cmd.Parameters.AddWithValue("@gid", graphId);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            return graphId;
        }

        public Graph LoadGraph(uint id)
        {
            bool estOriente = true;

            using (var conn = OpenConnection())
            {
                string sqlGraph = "SELECT est_oriente FROM Graphe WHERE id = @id;";

                using (var cmd = new MySqlCommand(sqlGraph, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                        estOriente = Convert.ToBoolean(result);
                }
            }

            Graph g = new Graph(estOriente);

            using (var conn = OpenConnection())
            {
                Dictionary<uint, string> idToNomSommet = new Dictionary<uint, string>();

                string sqlSommets = @"
                    SELECT id, nom, valeur, ordre
                    FROM Sommet
                    WHERE graphe_id = @gid
                    ORDER BY ordre;";

                using (var cmd = new MySqlCommand(sqlSommets, conn))
                {
                    cmd.Parameters.AddWithValue("@gid", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint sommetId = Convert.ToUInt32(reader["id"]);
                            string nom = reader["nom"].ToString();
                            float valeur = Convert.ToSingle(reader["valeur"]);

                            g.AddVertex(nom, valeur);
                            idToNomSommet[sommetId] = nom;
                        }
                    }
                }

                string sqlArcs = @"
                    SELECT source_id, destination_id, poids
                    FROM Arc
                    WHERE graphe_id = @gid;";

                using (var cmd = new MySqlCommand(sqlArcs, conn))
                {
                    cmd.Parameters.AddWithValue("@gid", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uint sourceId = Convert.ToUInt32(reader["source_id"]);
                            uint destId = Convert.ToUInt32(reader["destination_id"]);
                            float poids = Convert.ToSingle(reader["poids"]);

                            string sourceNom = idToNomSommet[sourceId];
                            string destNom = idToNomSommet[destId];

                            if (sourceNom != destNom)
                            {
                                try
                                {
                                    g.AddEdge(sourceNom, destNom, poids);
                                }
                                catch (ArgumentException)
                                {
                                    try
                                    {
                                        g.SetEdgeWeight(sourceNom, destNom, poids);
                                    }
                                    catch (ArgumentException)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return g;
        }

        public uint SaveTour(uint graphId, Tour t)
        {
            uint tourId;

            using (var conn = OpenConnection())
            {
                string sqlTour = @"
                    INSERT INTO Tournee(cout_total, graphe_id)
                    VALUES (@c, @gid);
                    SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sqlTour, conn))
                {
                    cmd.Parameters.AddWithValue("@c", t.Cost);
                    cmd.Parameters.AddWithValue("@gid", graphId);
                    tourId = Convert.ToUInt32(cmd.ExecuteScalar());
                }

                for (int i = 0; i < t.trajets.Count; i++)
                {
                    var segment = t.trajets[i];

                    string sqlStep = @"
                        INSERT INTO EtapeTournee(tournee_id, ordre, source, destination)
                        VALUES (@tid, @ordre, @source, @destination);";

                    using (var cmd = new MySqlCommand(sqlStep, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", tourId);
                        cmd.Parameters.AddWithValue("@ordre", i);
                        cmd.Parameters.AddWithValue("@source", segment.source);
                        cmd.Parameters.AddWithValue("@destination", segment.destination);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return tourId;
        }

        public Tour LoadTour(uint id)
        {
            Tour t = new Tour();

            using (var conn = OpenConnection())
            {
                string sqlCost = "SELECT cout_total FROM Tournee WHERE id = @id;";

                using (var cmd = new MySqlCommand(sqlCost, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                        t.Cost = Convert.ToSingle(result);
                }

                string sqlSteps = @"
            SELECT source, destination
            FROM EtapeTournee
            WHERE tournee_id = @tid
            ORDER BY ordre;";

                List<string> sequence = new List<string>();

                using (var cmd = new MySqlCommand(sqlSteps, conn))
                {
                    cmd.Parameters.AddWithValue("@tid", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string source = reader["source"].ToString();
                            string destination = reader["destination"].ToString();

                            if (sequence.Count == 0)
                                sequence.Add(source);

                            sequence.Add(destination);
                        }
                    }
                }

                for (int i = 0; i < sequence.Count - 1; i++)
                {
                    t.trajets.Add((sequence[i], sequence[i + 1]));
                }
            }

            return t;
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}