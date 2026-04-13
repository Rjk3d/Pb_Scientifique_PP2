namespace TourneeFutee
{
    public class Little
    {
        private Graph graphe;
        private Matrix matriceInitiale;
        private string[] indiceVersNom;

        private float meilleurCout;
        private Tour meilleureTournee;

        public Little(Graph graph)
        {
            this.graphe = graph;
            int n = graph.Order;

            matriceInitiale = new Matrix(n, n, float.PositiveInfinity);
            indiceVersNom = new string[n];

            foreach (var paire in graph.sommetsDico)
            {
                indiceVersNom[paire.Value] = paire.Key;
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matriceInitiale.SetValue(i, j, float.PositiveInfinity);
                    }
                    else
                    {
                        float poids = graph.matAdjacence.GetValue(i, j);
                        if (poids == graph.valeurPasDArc)
                        {
                            matriceInitiale.SetValue(i, j, float.PositiveInfinity);
                        }
                        else
                        {
                            matriceInitiale.SetValue(i, j, poids);
                        }
                    }
                }
            }
        }

        public Tour ComputeOptimalTour()
        {
            meilleurCout = float.PositiveInfinity;
            meilleureTournee = new Tour();

            Matrix matriceDepart = CopierMatrice(matriceInitiale);
            float borneInitiale = ReduceMatrix(matriceDepart);

            List<(string, string)> trajetsActuels = new List<(string, string)>();

            SeparationEvaluation(matriceDepart, borneInitiale, trajetsActuels);

            return meilleureTournee;
        }

        public static float ReduceMatrix(Matrix m)
        {
            float reductionTotale = 0;
            int n = m.NbRows;

            for (int i = 0; i < n; i++)
            {
                float minLigne = float.PositiveInfinity;
                for (int j = 0; j < n; j++)
                {
                    if (m.GetValue(i, j) < minLigne) minLigne = m.GetValue(i, j);
                }

                if (minLigne != float.PositiveInfinity && minLigne > 0)
                {
                    reductionTotale += minLigne;
                    for (int j = 0; j < n; j++)
                    {
                        if (m.GetValue(i, j) != float.PositiveInfinity)
                        {
                            m.SetValue(i, j, m.GetValue(i, j) - minLigne);
                        }
                    }
                }
            }

            for (int j = 0; j < n; j++)
            {
                float minColonne = float.PositiveInfinity;
                for (int i = 0; i < n; i++)
                {
                    if (m.GetValue(i, j) < minColonne) minColonne = m.GetValue(i, j);
                }

                if (minColonne != float.PositiveInfinity && minColonne > 0)
                {
                    reductionTotale += minColonne;
                    for (int i = 0; i < n; i++)
                    {
                        if (m.GetValue(i, j) != float.PositiveInfinity)
                        {
                            m.SetValue(i, j, m.GetValue(i, j) - minColonne);
                        }
                    }
                }
            }

            return reductionTotale;
        }

        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            float regretMaximal = -1f;
            int meilleurI = -1;
            int meilleurJ = -1;
            int n = m.NbRows;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (m.GetValue(i, j) == 0)
                    {
                        float minLigne = float.PositiveInfinity;
                        for (int c = 0; c < n; c++)
                        {
                            if (c != j && m.GetValue(i, c) < minLigne) minLigne = m.GetValue(i, c);
                        }

                        float minColonne = float.PositiveInfinity;
                        for (int r = 0; r < n; r++)
                        {
                            if (r != i && m.GetValue(r, j) < minColonne) minColonne = m.GetValue(r, j);
                        }

                        float regret = 0;
                        if (minLigne != float.PositiveInfinity) regret += minLigne;
                        if (minColonne != float.PositiveInfinity) regret += minColonne;

                        if (regret > regretMaximal)
                        {
                            regretMaximal = regret;
                            meilleurI = i;
                            meilleurJ = j;
                        }
                    }
                }
            }

            return (meilleurI, meilleurJ, regretMaximal);
        }

        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {
            var tousLesTrajets = new List<(string, string)>(includedSegments);
            tousLesTrajets.Add(segment);

            string courant = segment.Item2;
            int longueurCycle = 1;

            while (true)
            {
                bool suivantTrouve = false;
                foreach (var trajet in tousLesTrajets)
                {
                    if (trajet.Item1 == courant)
                    {
                        courant = trajet.Item2;
                        longueurCycle++;
                        suivantTrouve = true;
                        break;
                    }
                }

                if (!suivantTrouve)
                {
                    return false;
                }

                if (courant == segment.Item1)
                {
                    if (longueurCycle < nbCities)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void SeparationEvaluation(Matrix m, float borneCourante, List<(string, string)> trajetsActuels)
        {
            if (borneCourante >= meilleurCout) return;

            int n = m.NbRows;

            if (trajetsActuels.Count == n)
            {
                if (borneCourante < meilleurCout)
                {
                    meilleurCout = borneCourante;
                    meilleureTournee = new Tour();
                    meilleureTournee.Cost = meilleurCout;
                    meilleureTournee.trajets = new List<(string, string)>(trajetsActuels);
                }
                return;
            }

            var regretMax = GetMaxRegret(m);
            int meilleurI = regretMax.i;
            int meilleurJ = regretMax.j;

            if (meilleurI == -1 || meilleurJ == -1) return;

            string villeSource = indiceVersNom[meilleurI];
            string villeDest = indiceVersNom[meilleurJ];

            Matrix matriceInclusion = CopierMatrice(m);

            for (int c = 0; c < n; c++) matriceInclusion.SetValue(meilleurI, c, float.PositiveInfinity);
            for (int r = 0; r < n; r++) matriceInclusion.SetValue(r, meilleurJ, float.PositiveInfinity);

            var trajetsInclusion = new List<(string, string)>(trajetsActuels);
            trajetsInclusion.Add((villeSource, villeDest));

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (matriceInclusion.GetValue(r, c) != float.PositiveInfinity)
                    {
                        var candidat = (indiceVersNom[r], indiceVersNom[c]);
                        if (IsForbiddenSegment(candidat, trajetsInclusion, n))
                        {
                            matriceInclusion.SetValue(r, c, float.PositiveInfinity);
                        }
                    }
                }
            }

            float reductionInclusion = ReduceMatrix(matriceInclusion);
            SeparationEvaluation(matriceInclusion, borneCourante + reductionInclusion, trajetsInclusion);

            Matrix matriceExclusion = CopierMatrice(m);

            matriceExclusion.SetValue(meilleurI, meilleurJ, float.PositiveInfinity);

            float reductionExclusion = ReduceMatrix(matriceExclusion);
            SeparationEvaluation(matriceExclusion, borneCourante + reductionExclusion, trajetsActuels);
        }

        private static Matrix CopierMatrice(Matrix m)
        {
            Matrix copie = new Matrix(m.NbRows, m.NbColumns, m.DefaultValue);
            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    copie.SetValue(i, j, m.GetValue(i, j));
                }
            }
            return copie;
        }
    }
}