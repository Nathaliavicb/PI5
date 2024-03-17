using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace pi5.entities;

[Table ("valores")]
public class Valores{
    public int Id{ get ; set;}
    public int Ação_id{ get ; set;}
    public DateTime Data{ get ; set;}
    public float Valor_Fechamento{ get ; set;}
    public float Valor_Abertura{ get ; set;}
    public float Valor_Alta{ get ; set;}
    public float Valor_Baixa{ get ; set;}

}