using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prueba.Migrations.PruebaMigrations
{
    public partial class changesPuestosE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "PuestoE",
                columns: table => new
                {
                    id_puestoE = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_estacionamiento = table.Column<int>(type: "int", nullable: false),
                    id_propiedad = table.Column<int>(type: "int", nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Alicuota = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puesto_E", x => x.id_puestoE);
                    table.ForeignKey(
                        name: "FK_Puesto_E_Estacionamiento",
                        column: x => x.id_estacionamiento,
                        principalTable: "Estacionamiento",
                        principalColumn: "id_estacionamiento");
                    table.ForeignKey(
                        name: "FK_Puesto_E_Propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "Propiedad",
                        principalColumn: "id_propiedad");
                });
        }
            
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "PuestoE");

        }
    }
}
