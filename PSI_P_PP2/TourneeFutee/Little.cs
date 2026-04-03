using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        //  TODO: ajouter tous les attributs que vous jugerez pertinents 
        private Graph graph;
        private Tour chemin;
        private float minimum;

        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
            // TODO : implémenter
            this.graph = graph;
            this.chemin = null;
            this.minimum = float.PositiveInfinity;
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            // TODO : implémenter
            List<string> sommets = GetVerticesNames();

            if (sommets.Count == 0)
            {
                return new Tour();
            }

            string start = sommets[0];
            List<string> autres = new List<string>();

            for (int i = 1; i < sommets.Count; i++)
            {
                autres.Add(sommets[i]);
            }

            List<string> permutationCourante = new List<string>();
            List<string> meilleurePermutation = null;

            Permute(autres, permutationCourante, ref meilleurePermutation, start);

            Tour tour = new Tour();

            if (meilleurePermutation == null)
            {
                return tour;
            }

            string courant = start;
            for (int i = 0; i < meilleurePermutation.Count; i++)
            {
                string suivant = meilleurePermutation[i];
                float cout = graph.GetEdgeWeight(courant, suivant);
                tour.AddSegment(courant, suivant, cout);
                courant = suivant;
            }

            float coutRetour = graph.GetEdgeWeight(courant, start);
            tour.AddSegment(courant, start, coutRetour);

            this.chemin = tour;
            return tour;
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            // TODO : implémenter
            float reduction = 0.0f;

            // Réduction des lignes
            for (int i = 0; i < m.NbRows; i++)
            {
                float min = float.PositiveInfinity;

                for (int j = 0; j < m.NbColumns; j++)
                {
                    float val = m.GetValue(i, j);
                    if (val < min)
                    {
                        min = val;
                    }
                }

                if (min != float.PositiveInfinity && min > 0)
                {
                    for (int j = 0; j < m.NbColumns; j++)
                    {
                        float val = m.GetValue(i, j);
                        if (val != float.PositiveInfinity)
                        {
                            m.SetValue(i, j, val - min);
                        }
                    }

                    reduction += min;
                }
            }

            // Réduction des colonnes
            for (int j = 0; j < m.NbColumns; j++)
            {
                float min = float.PositiveInfinity;

                for (int i = 0; i < m.NbRows; i++)
                {
                    float val = m.GetValue(i, j);
                    if (val < min)
                    {
                        min = val;
                    }
                }

                if (min != float.PositiveInfinity && min > 0)
                {
                    for (int i = 0; i < m.NbRows; i++)
                    {
                        float val = m.GetValue(i, j);
                        if (val != float.PositiveInfinity)
                        {
                            m.SetValue(i, j, val - min);
                        }
                    }

                    reduction += min;
                }
            }

            return reduction;
        }

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            // TODO : implémenter
            int bestI = 0;
            int bestJ = 0;
            float maxRegret = -1.0f;

            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) == 0)
                    {
                        float minLigne = float.PositiveInfinity;
                        for (int k = 0; k < m.NbColumns; k++)
                        {
                            if (k != j)
                            {
                                float v = m.GetValue(i, k);
                                if (v < minLigne)
                                {
                                    minLigne = v;
                                }
                            }
                        }

                        float minColonne = float.PositiveInfinity;
                        for (int k = 0; k < m.NbRows; k++)
                        {
                            if (k != i)
                            {
                                float v = m.GetValue(k, j);
                                if (v < minColonne)
                                {
                                    minColonne = v;
                                }
                            }
                        }

                        float regret = minLigne + minColonne;

                        if (regret > maxRegret)
                        {
                            maxRegret = regret;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
            }

            return (bestI, bestJ, maxRegret);

        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {

            // TODO : implémenter

            // trajet inverse direct
            foreach ((string source, string destination) s in includedSegments)
            {
                if (s.source == segment.destination && s.destination == segment.source)
                {
                    return true;
                }
            }

            Dictionary<string, string> suivants = new Dictionary<string, string>();

            foreach ((string source, string destination) s in includedSegments)
            {
                if (!suivants.ContainsKey(s.source))
                {
                    suivants.Add(s.source, s.destination);
                }
            }

            if (suivants.ContainsKey(segment.source))
            {
                return true;
            }

            suivants[segment.source] = segment.destination;

            string courant = segment.destination;
            int longueurCycle = 1;

            while (suivants.ContainsKey(courant))
            {
                courant = suivants[courant];
                longueurCycle++;

                if (courant == segment.source)
                {
                    if (longueurCycle < nbCities)
                    {
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

        private List<string> GetVerticesNames()
        {
            List<string> noms = new List<string>(new string[graph.Order]);

            foreach (KeyValuePair<string, int> kvp in graph.sommetsDico)
            {
                noms[kvp.Value] = kvp.Key;
            }

            return noms;
        }

        private void Permute(List<string> restants, List<string> courant, ref List<string> meilleur, string start)
        {
            if (restants.Count == 0)
            {
                float cout = ComputePathCost(start, courant);

                if (cout < minimum)
                {
                    minimum = cout;
                    meilleur = new List<string>(courant);
                }
                return;
            }

            for (int i = 0; i < restants.Count; i++)
            {
                string ville = restants[i];
                courant.Add(ville);
                restants.RemoveAt(i);

                Permute(restants, courant, ref meilleur, start);

                restants.Insert(i, ville);
                courant.RemoveAt(courant.Count - 1);
            }
        }

        private float ComputePathCost(string start, List<string> chemin)
        {
            float cout = 0.0f;
            string courant = start;

            for (int i = 0; i < chemin.Count; i++)
            {
                cout += graph.GetEdgeWeight(courant, chemin[i]);
                courant = chemin[i];
            }

            cout += graph.GetEdgeWeight(courant, start);
            return cout;
        }
    }
}