using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase5Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Loans_ApplicantId",
                table: "Loans",
                column: "ApplicantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Applicants_ApplicantId",
                table: "Loans",
                column: "ApplicantId",
                principalTable: "Applicants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Repayments_Loans_LoanId",
                table: "Repayments",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Applicants_ApplicantId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Repayments_Loans_LoanId",
                table: "Repayments");

            migrationBuilder.DropIndex(
                name: "IX_Loans_ApplicantId",
                table: "Loans");
        }
    }
}
