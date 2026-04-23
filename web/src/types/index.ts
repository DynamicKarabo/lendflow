export interface Applicant {
  id: string;
  tenantId: string;
  firstName: string;
  lastName: string;
  idNumber: string;
  phoneNumber: string;
  email: string;
  dateOfBirth: string;
  employmentStatus: string;
  monthlyIncome: number;
  monthlyExpenses: number;
}

export type LoanApplicationStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected"
  | "Cancelled";

export interface LoanApplication {
  id: string;
  applicantId: string;
  applicantName?: string;
  status: LoanApplicationStatus;
  requestedAmount: number;
  requestedTermMonths: number;
  purpose: string;
  creditScore?: number;
  riskBand?: string;
  submittedAt?: string;
}

export type LoanStatus =
  | "PendingDisbursement"
  | "Active"
  | "Settled"
  | "Defaulted";

export interface Loan {
  id: string;
  applicationId: string;
  applicantId: string;
  applicantName?: string;
  principal: number;
  interestRate: number;
  termMonths: number;
  status: LoanStatus;
  outstandingBalance: number;
  monthlyInstallment: number;
  disbursementDate?: string;
  createdAt: string;
}

export type RepaymentStatus = "Scheduled" | "Paid" | "Overdue";

export interface Repayment {
  id: string;
  loanId: string;
  installmentNumber: number;
  amountDue: number;
  amountPaid: number;
  status: RepaymentStatus;
  dueDate: string;
  paidDate?: string;
  paymentReference?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardStats {
  totalLoans: number;
  totalOutstanding: number;
  totalDisbursed: number;
  activeApplications: number;
  approvalRate: number;
  avgLoanAmount: number;
}
