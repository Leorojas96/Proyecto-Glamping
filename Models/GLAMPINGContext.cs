using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Glamping2.Models
{
    public partial class GLAMPINGContext : DbContext
    {
        public GLAMPINGContext()
        {
        }

        public GLAMPINGContext(DbContextOptions<GLAMPINGContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Habitacione> Habitaciones { get; set; } = null!;
        public virtual DbSet<Paquete> Paquetes { get; set; } = null!;
        public virtual DbSet<PaqueteServicio> PaqueteServicios { get; set; } = null!;
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Persona> Personas { get; set; } = null!;
        public virtual DbSet<Reserva> Reservas { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<RolesPermiso> RolesPermisos { get; set; } = null!;
        public virtual DbSet<Servicio> Servicios { get; set; }
        public virtual DbSet<TipoHabitacion> TipoHabitacions { get; set; } = null!;
        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    // Usa el archivo de configuración para la cadena de conexión
                    "Name=ConnectionStrings:DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Habitacione>(entity =>
            {
                entity.HasKey(e => e.IdHabitacion)
                    .HasName("PK__Habitaci__8BBBF9010375ED00");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.EstadoHabitacion)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdTipoHabitaNavigation)
                    .WithMany(p => p.Habitaciones)
                    .HasForeignKey(d => d.IdTipoHabita)
                    .HasConstraintName("FK__Habitacio__IdTip__5AEE82B9");
            });

            modelBuilder.Entity<Paquete>(entity =>
            {
                entity.HasKey(e => e.IdPaquetes)
                    .HasName("PK__Paquetes__E6DFAA1EAB7B1709");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NomPaquete)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdHabitacionNavigation)
                    .WithMany(p => p.Paquetes)
                    .HasForeignKey(d => d.IdHabitacion)
                    .HasConstraintName("FK__Paquetes__IdHabi__619B8048");

                entity.HasOne(d => d.IdServiciosNavigation)
                    .WithMany(p => p.Paquetes)
                    .HasForeignKey(d => d.IdServicios)
                    .HasConstraintName("FK__Paquetes__IdServ__60A75C0F");
            });

            modelBuilder.Entity<PaqueteServicio>(entity =>
            {
                entity.HasKey(e => e.IdPaqueteServicio)
                    .HasName("PK__PaqueteS__DE5C2BC2D9796B4F");

                entity.ToTable("PaqueteServicio");

                entity.HasOne(d => d.IdPaquetesNavigation)
                    .WithMany(p => p.PaqueteServicios)
                    .HasForeignKey(d => d.IdPaquetes)
                    .HasConstraintName("FK__PaqueteSe__IdPaq__656C112C");

                entity.HasOne(d => d.IdServiciosNavigation)
                    .WithMany(p => p.PaqueteServicios)
                    .HasForeignKey(d => d.IdServicios)
                    .HasConstraintName("FK__PaqueteSe__IdSer__6477ECF3");
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.HasKey(e => e.IdPermisos)
                    .HasName("PK__Permisos__CE7ED38DAEBBFBC7");

                entity.Property(e => e.EstadoPermiso)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NomPermiso)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Persona>(entity =>
            {
                entity.HasKey(e => e.IdPersona)
                    .HasName("PK__Personas__2EC8D2AC4C95E7C0");

                entity.Property(e => e.ApePersona)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Ciudad)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Direcion)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FechaNaci).HasColumnType("date");

                entity.Property(e => e.Nacionalidad)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NomPersona)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TipoDoc)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.IdReservas)
                    .HasName("PK__Reservas__1549E3076D71AFB9");

                entity.Property(e => e.EstadoReserva)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FechaFin).HasColumnType("date");

                entity.Property(e => e.FechaInicio).HasColumnType("date");

                entity.Property(e => e.FechaReserva).HasColumnType("date");

                entity.HasOne(d => d.IdPaquetesNavigation)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.IdPaquetes)
                    .HasConstraintName("FK__Reservas__IdPaqu__693CA210");

                entity.HasOne(d => d.IdUsuarioNavigation)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.IdUsuario)
                    .HasConstraintName("FK__Reservas__IdUsua__68487DD7");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.IdRol)
                    .HasName("PK__Roles__2A49584CD554B2D5");

                entity.Property(e => e.NomRol)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RolesPermiso>(entity =>
            {
                entity.HasKey(e => e.IdRolesPermisos)
                    .HasName("PK__RolesPer__AAB7C825CAC164C3");

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdPermisosNavigation)
                    .WithMany(p => p.RolesPermisos)
                    .HasForeignKey(d => d.IdPermisos)
                    .HasConstraintName("FK__RolesPerm__IdPer__4D94879B");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.RolesPermisos)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__RolesPerm__IdRol__4E88ABD4");
            });

            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.HasKey(e => e.IdServicios)
                    .HasName("PK__Servicio__0113729928969BC7");

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.NomServicio)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TipoServicio)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TipoHabitacion>(entity =>
            {
                entity.HasKey(e => e.IdTipoHabita)
                    .HasName("PK__TipoHabi__EA7CCDADEA0E1A6E");

                entity.ToTable("TipoHabitacion");

                entity.Property(e => e.Estado)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario)
                    .HasName("PK__Usuario__5B65BF97CBB511E1");

                entity.ToTable("Usuario");

                entity.Property(e => e.Contraseña)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Correo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdPersonaNavigation)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.IdPersona)
                    .HasConstraintName("FK__Usuario__IdPerso__534D60F1");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__Usuario__IdRol__5441852A");
            });

            OnModelCreatingPartial(modelBuilder);
        }   

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
