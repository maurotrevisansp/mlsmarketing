using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLSMarketing
{
    public class mlsPesq
    {
        public string Link { get; set; }
        public string Titulo { get; set; }
        public string Imagem { get; set; }
        public string Tipo { get; set; }
        public string Concelho { get; set; }
        public string Endereco { get; set; }
        public string Id { get; set; }
        public string Quartos { get; set; }
        public string Suites { get; set; }
        public string Banho { get; set; }
        public string Garagem { get; set; }
        public string Area { get; set; }
        public string AreaB { get; set; }
        public string Valor { get; set; }
        public string Offer { get; set; }
        public string Descricao { get; set; }
        public string Telefone { get; set; }
        public string Nome { get; set; }
        public string TipoAnunciante { get; set; }

        public virtual List<mlsDetails> MlsDetails { get; set; }

    }
}
