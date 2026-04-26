using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        public Matrix matAdjacence;
        public Dictionary<string, int> sommetsDico;
        public List<float> valeursSommets;
        public bool estOriente;
        public float valeurPasDArc;

        public Graph(bool directed, float noEdgeValue = 0)
        {
            estOriente = directed;
            valeurPasDArc = noEdgeValue;
            matAdjacence = new Matrix(0, 0, noEdgeValue);
            sommetsDico = new Dictionary<string, int>();
            valeursSommets = new List<float>();
        }

        public int Order
        {
            get { return sommetsDico.Count; }
        }
        public bool Directed
        {
            get { return estOriente; }
        }

        public bool ContainsVertex(string name)
        {
            return sommetsDico.ContainsKey(name);
        }
        public void AddVertex(string name, float value = 0)
        {
            if (sommetsDico.ContainsKey(name)) throw new ArgumentException();

            int index = valeursSommets.Count;
            sommetsDico.Add(name, index);
            valeursSommets.Add(value);

            matAdjacence.AddRow(index);
            matAdjacence.AddColumn(index);
        }

        public void RemoveVertex(string name)
        {
            if (sommetsDico.ContainsKey(name) == false) throw new ArgumentException();

            int indexASupprimer = sommetsDico[name];

            matAdjacence.RemoveRow(indexASupprimer);
            matAdjacence.RemoveColumn(indexASupprimer);

            valeursSommets.RemoveAt(indexASupprimer);
            sommetsDico.Remove(name);

            List<string> cles = new List<string>(sommetsDico.Keys);
            for (int k = 0; k < cles.Count; k++)
            {
                string cle = cles[k];
                if (sommetsDico[cle] > indexASupprimer)
                {
                    sommetsDico[cle] = sommetsDico[cle] - 1;
                }
            }
        }

        public void AddEdge(string source, string dest, float weight = 1)
        {
            if (sommetsDico.ContainsKey(source) == false || sommetsDico.ContainsKey(dest) == false)
                throw new ArgumentException();

            int i = sommetsDico[source];
            int j = sommetsDico[dest];

            if (matAdjacence.GetValue(i, j) != valeurPasDArc) throw new ArgumentException();

            matAdjacence.SetValue(i, j, weight);
            if (estOriente == false)
            {
                matAdjacence.SetValue(j, i, weight);
            }
        }

        public void RemoveEdge(string source, string dest)
        {
            if (sommetsDico.ContainsKey(source) == false || sommetsDico.ContainsKey(dest) == false)
                throw new ArgumentException();

            int i = sommetsDico[source];
            int j = sommetsDico[dest];

            if (matAdjacence.GetValue(i, j) == valeurPasDArc) throw new ArgumentException();

            matAdjacence.SetValue(i, j, valeurPasDArc);
            if (estOriente == false)
            {
                matAdjacence.SetValue(j, i, valeurPasDArc);
            }
        }

        public float GetEdgeWeight(string source, string dest)
        {
            if (sommetsDico.ContainsKey(source) == false || sommetsDico.ContainsKey(dest) == false)
                throw new ArgumentException();

            int i = sommetsDico[source];
            int j = sommetsDico[dest];

            float poids = matAdjacence.GetValue(i, j);
            if (poids == valeurPasDArc) throw new ArgumentException();

            return poids;
        }

        public void SetEdgeWeight(string source, string dest, float weight)
        {
            if (sommetsDico.ContainsKey(source) == false || sommetsDico.ContainsKey(dest) == false)
                throw new ArgumentException();

            int i = sommetsDico[source];
            int j = sommetsDico[dest];

            if (matAdjacence.GetValue(i, j) == valeurPasDArc) throw new ArgumentException();

            matAdjacence.SetValue(i, j, weight);
            if (estOriente == false)
            {
                matAdjacence.SetValue(j, i, weight);
            }
        }

        public List<string> GetNeighbors(string name)
        {
            if (sommetsDico.ContainsKey(name) == false) throw new ArgumentException();

            int i = sommetsDico[name];
            List<string> voisins = new List<string>();

            for (int j = 0; j < matAdjacence.NbColumns; j++)
            {
                if (matAdjacence.GetValue(i, j) != valeurPasDArc)
                {
                    foreach (var entree in sommetsDico)
                    {
                        if (entree.Value == j)
                        {
                            voisins.Add(entree.Key);
                            break;
                        }
                    }
                }
            }
            return voisins;
        }

        public float GetVertexValue(string name)
        {
            if (sommetsDico.ContainsKey(name) == false) throw new ArgumentException();
            return valeursSommets[sommetsDico[name]];
        }

        public void SetVertexValue(string name, float val)
        {
            if (sommetsDico.ContainsKey(name) == false) throw new ArgumentException();
            valeursSommets[sommetsDico[name]] = val;
        }
    }
}