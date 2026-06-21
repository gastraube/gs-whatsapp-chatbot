using Microsoft.EntityFrameworkCore;
using gschatbot.api.Models;

namespace gschatbot.api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Especialidade> Especialidades { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<Especialista> Especialistas { get; set; }
    public DbSet<HorarioConsulta> HorariosConsulta { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }
    public DbSet<HistoricoMensagem> HistoricoMensagens { get; set; }
    public DbSet<PlanoAssistencia> PlanosAssistencia { get; set; }
    public DbSet<EspecialistaPlano> EspecialistaPlanos { get; set; }
    public DbSet<ClientePlano> ClientePlanos { get; set; }
    public DbSet<ClienteNumero> ClienteNumeros { get; set; }
    public DbSet<MetodoPagamento> MetodosPagamento { get; set; }
    public DbSet<EspecialistaMetodoPagamento> EspecialistaMetodosPagamento { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Especialidade -> Especialista (1:N)
        modelBuilder.Entity<Especialista>()
            .HasOne(e => e.Especialidade)
            .WithMany(esp => esp.Especialistas)
            .HasForeignKey(e => e.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        // HorarioConsulta -> Especialista (1:N)
        modelBuilder.Entity<HorarioConsulta>()
            .HasOne(hc => hc.Especialista)
            .WithMany(e => e.HorariosConsulta)
            .HasForeignKey(hc => hc.EspecialistaId)
            .OnDelete(DeleteBehavior.Cascade);

        // HorarioConsulta -> Endereco (N:1)
        modelBuilder.Entity<HorarioConsulta>()
            .HasOne(hc => hc.Endereco)
            .WithMany(end => end.HorariosConsulta)
            .HasForeignKey(hc => hc.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);

        // HorarioConsulta <-> Agendamento (1:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.HorarioConsulta)
            .WithOne(hc => hc.Agendamento)
            .HasForeignKey<Agendamento>(a => a.HorarioConsultaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agendamento -> Cliente (N:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Agendamentos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Agendamento -> Especialista (N:1)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.Especialista)
            .WithMany(e => e.Agendamentos)
            .HasForeignKey(a => a.EspecialistaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agendamento -> PlanoAssistencia (N:1, opcional)
        modelBuilder.Entity<Agendamento>()
            .HasOne(a => a.PlanoAssistencia)
            .WithMany()
            .HasForeignKey(a => a.PlanoAssistenciaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // HistoricoMensagem -> Cliente (N:1)
        modelBuilder.Entity<HistoricoMensagem>()
            .HasOne(hm => hm.Cliente)
            .WithMany(c => c.HistoricoMensagens)
            .HasForeignKey(hm => hm.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // HistoricoMensagem -> Agendamento (N:1, opcional)
        modelBuilder.Entity<HistoricoMensagem>()
            .HasOne(hm => hm.Agendamento)
            .WithMany(a => a.HistoricoMensagens)
            .HasForeignKey(hm => hm.AgendamentoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // EspecialistaPlano — chave composta
        modelBuilder.Entity<EspecialistaPlano>()
            .HasKey(ep => new { ep.EspecialistaId, ep.PlanoAssistenciaId });

        modelBuilder.Entity<EspecialistaPlano>()
            .HasOne(ep => ep.Especialista)
            .WithMany(e => e.PlanosAtendidos)
            .HasForeignKey(ep => ep.EspecialistaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EspecialistaPlano>()
            .HasOne(ep => ep.PlanoAssistencia)
            .WithMany(p => p.Especialistas)
            .HasForeignKey(ep => ep.PlanoAssistenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClientePlano — chave composta
        modelBuilder.Entity<ClientePlano>()
            .HasKey(cp => new { cp.ClienteId, cp.PlanoAssistenciaId });

        modelBuilder.Entity<ClientePlano>()
            .HasOne(cp => cp.Cliente)
            .WithMany(c => c.Planos)
            .HasForeignKey(cp => cp.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClientePlano>()
            .HasOne(cp => cp.PlanoAssistencia)
            .WithMany(p => p.Clientes)
            .HasForeignKey(cp => cp.PlanoAssistenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClienteNumero -> Cliente (N:1)
        modelBuilder.Entity<ClienteNumero>()
            .HasOne(cn => cn.Cliente)
            .WithMany(c => c.Numeros)
            .HasForeignKey(cn => cn.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClienteNumero>()
            .HasIndex(cn => cn.Numero)
            .IsUnique();

        // EspecialistaMetodoPagamento — chave composta
        modelBuilder.Entity<EspecialistaMetodoPagamento>()
            .HasKey(emp => new { emp.EspecialistaId, emp.MetodoPagamentoId });

        modelBuilder.Entity<EspecialistaMetodoPagamento>()
            .HasOne(emp => emp.Especialista)
            .WithMany(e => e.MetodosPagamento)
            .HasForeignKey(emp => emp.EspecialistaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EspecialistaMetodoPagamento>()
            .HasOne(emp => emp.MetodoPagamento)
            .WithMany(m => m.Especialistas)
            .HasForeignKey(emp => emp.MetodoPagamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // StatusAgendamento como int no banco
        modelBuilder.Entity<Agendamento>()
            .Property(a => a.Status)
            .HasConversion<int>();

        // Índices para performance
        modelBuilder.Entity<HorarioConsulta>()
            .HasIndex(hc => new { hc.EspecialistaId, hc.DataConsulta });

        modelBuilder.Entity<Agendamento>()
            .HasIndex(a => a.ClienteId);

        modelBuilder.Entity<Agendamento>()
            .HasIndex(a => a.Status);

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Cpf)
            .IsUnique();
    }
}
