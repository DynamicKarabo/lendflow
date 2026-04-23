export interface Applicant {
  Id: string;
  TenantId: string;
  FirstName: string;
  LastName: string;
  IdNumber: string;
  PhoneNumber: string;
  Email: string;
  DateOfBirth: string;
  EmploymentStatus: string;
  MonthlyIncome: number;
  MonthlyExpenses: number;
  CreatedAt: string;
  UpdatedAt?: string;
}

export type LoanApplicationStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected"
  | "Cancelled";

export interface LoanApplication {
  Id: string;
  ApplicantId: string;
  ApplicantName: string;
  Status: LoanApplicationStatus;
  RequestedAmount: number;
  RequestedTermMonths: number;
  Purpose: string;
  CreditScore?: number;
  RiskBand?: string;
  CreatedAt: string;
  UpdatedAt?: string;
}

export type LoanStatus =
  | "PendingDisbursement"
  | "Active"
  | "Settled"
  | "Defaulted";

export interface Loan {
  Id: string;
  ApplicationId: string;
  ApplicantId: string;
  ApplicantName: string;
  PrincipalAmount: number;
  InterestRate: number;
  TermMonths: number;
  RepaymentFrequency: string;
  Status: LoanStatus;
  OutstandingBalance: number;
  MonthlyInstallment: number;
  DisbursementDate?: string;
  MaturityDate: string;
  CreatedAt: string;
}

export type RepaymentStatus = "Scheduled" | "Paid" | "Overdue";

export interface Repayment {
  Id: string;
  InstallmentNumber: number;
  AmountDue: number;
  AmountPaid?: number;
  Status: RepaymentStatus;
  DueDate: string;
  PaidDate?: string;
  PaymentReference?: string;
  CreatedAt: string;
}

export interface PagedResult<T> {
  Items: T[];
  TotalCount: number;
  PageNumber: number;
  PageSize: number;
}

export interface LoginRequest {
  Email: string;
  Password: string;
}

export interface LoginResponse {
  Token: string;
  Email: string;
  Role: string;
}
