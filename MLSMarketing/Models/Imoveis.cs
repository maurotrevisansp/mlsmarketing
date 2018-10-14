using System;

namespace MLSMarketing
{
    public class Imoveis
    {
        public int Id_Imovel { get; set; }
        public string Descricao { get; set; }
        public int Id_Imobiliaria { get; set; }
        public int Id_Usuario { get; set; }
        public int Id_Owner { get; set; }
        public int Id_ProComp { get; set; }
        public int Id_Freguesia { get; set; }
        public string LinkImagemPrincipal { get; set; }
        public string LinkAnuncio { get; set; }
        public string property_title { get; set; }
        public string property_type { get; set; } // house, apartment, commercial
        public string property_offer { get; set; } // rent, sale
        public string property_rental_period_type { get; set; } // monthly, yearly 
        public decimal property_price { get; set; }
        public int property_area { get; set; }
        public int property_rental_period { get; set; }
        public string roperty_map_address { get; set; }
        public string property_address { get; set; }
        public string property_longtitude { get; set; }
        public string property_latitude { get; set; }
        public string property_city { get; set; }
        public string property_freguesia { get; set; }
        public string property_concelho { get; set; }
        public string property_zona { get; set; }
        public int property_year { get; set; }
        public int property_bedrooms { get; set; }
        public int property_suites { get; set; }
        public int property_bathrooms { get; set; }
        public int property_lot_size { get; set; }
        public int property_parking { get; set; }
        public string property_cooling { get; set; }
        public string property_sewer { get; set; }
        public string property_water { get; set; }
        public string property_exercise_room { get; set; }
        public string Id_Externo { get; set; }

        //novos campos
        public string property_estado_imovel { get; set; }
        public string property_estado_angariacao { get; set; }
        public DateTime? property_dt_incluido { get; set; }
        public DateTime? property_dt_alterado { get; set; }
        public DateTime? property_dt_venda { get; set; }
        public string property_andar { get; set; }
        public string property_referencia_cod { get; set; }
        public string property_avaliacao_preco { get; set; }

        // mais novos campos
        public decimal property_price_condominio { get; set; }
        public decimal property_valor_vendido { get; set; }
        public string property_Eficiencia_Energetica { get; set; }
        public string property_Observacoes { get; set; }
        public string property_HipotecaBanco { get; set; }
        public decimal property_HipotecaDivida { get; set; }

        public decimal property_comissao { get; set; }
        public string property_comissao_tipo { get; set; }
        public DateTime? property_dt_angariacao_inicio { get; set; }
        public DateTime? property_dt_angariacao_fim { get; set; }
        public DateTime? property_dt_compra { get; set; }


        public bool chkArCondicionado { get; set; }
        public bool chkSomAmbiente { get; set; }
        public bool chkEstoresEletricos { get; set; }
        public bool chkVidrosDuplos { get; set; }
        public bool chkArrecadacao { get; set; }
        public bool chkPortaBlindada { get; set; }
        public bool chkAspiracaoCentral { get; set; }
        public bool chkLareira { get; set; }
        public bool chkVideoPorteiro { get; set; }
        public bool chkGasCanalizado { get; set; }
        public bool chkHidromassagem { get; set; }
        public bool chkJacuzzi { get; set; }
        public bool chkAlarme { get; set; }
        public bool chkRegaAutomatica { get; set; }
        public bool chkPoco { get; set; }
        public bool chkFuroDagua { get; set; }
        public bool chkDespensa { get; set; }
        public bool chkPiscina { get; set; }


        //Novos Checkbox
        public bool chkGaragem { get; set; }
        public bool chkTerraco { get; set; }
        public bool chkJardim { get; set; }
        public bool chkPortaria { get; set; }
        public bool chkRural { get; set; }
        public bool chkInternet { get; set; }
        public bool chkBanhoExterno { get; set; }
        public bool chkQuintal { get; set; }
        public bool chkMicroondas { get; set; }
        public bool chkTvCabo { get; set; }
        public bool chkLavanderia { get; set; }
        public bool chkChuveiroExterno { get; set; }
        public bool chkBar { get; set; }
        public bool chkChurrasqueira { get; set; }
        public bool chkPomar { get; set; }
        public bool chkSacada { get; set; }
        public bool chkElevador { get; set; }
        public bool chkQuadra { get; set; }
        public bool chkVistaMar { get; set; }
        public bool chkCondominioFechado { get; set; }

    }
}
