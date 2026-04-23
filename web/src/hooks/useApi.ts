import { useState, useEffect } from "react"
import type {
  Applicant,
  Loan,
  LoanApplication,
  Repayment,
  DashboardStats,
} from "@/types"

// Mock data for premium demo
const mockApplicants: Applicant[] = [
  { id: "1", tenantId: "t1", firstName: "Thabo", lastName: "Mokoena", idNumber: "9001155000087", phoneNumber: "0823456789", email: "thabo.m@email.co.za", dateOfBirth: "1990-01-15", employmentStatus: "Employed", monthlyIncome: 35000, monthlyExpenses: 15000 },
  { id: "2", tenantId: "t1", firstName: "Lerato", lastName: "Dlamini", idNumber: "8807125000083", phoneNumber: "0834567890", email: "lerato.d@email.co.za", dateOfBirth: "1988-07-12", employmentStatus: "Self-employed", monthlyIncome: 28000, monthlyExpenses: 12000 },
  { id: "3", tenantId: "t1", firstName: "Sipho", lastName: "Nkosi", idNumber: "9503015000081", phoneNumber: "0845678901", email: "sipho.n@email.co.za", dateOfBirth: "1995-03-01", employmentStatus: "Employed", monthlyIncome: 22000, monthlyExpenses: 9000 },
  { id: "4", tenantId: "t1", firstName: "Nomsa", lastName: "Zulu", idNumber: "7804255000089", phoneNumber: "0856789012", email: "nomsa.z@email.co.za", dateOfBirth: "1978-04-25", employmentStatus: "Employed", monthlyIncome: 55000, monthlyExpenses: 25000 },
  { id: "5", tenantId: "t1", firstName: "Bongani", lastName: "Sithole", idNumber: "9208105000085", phoneNumber: "0867890123", email: "bongani.s@email.co.za", dateOfBirth: "1992-08-10", employmentStatus: "Unemployed", monthlyIncome: 8000, monthlyExpenses: 6000 },
]

const mockApplications: LoanApplication[] = [
  { id: "a1", applicantId: "1", applicantName: "Thabo Mokoena", status: "Approved", requestedAmount: 50000, requestedTermMonths: 24, purpose: "Home renovation", creditScore: 720, riskBand: "Low", submittedAt: "2024-01-15T10:00:00Z" },
  { id: "a2", applicantId: "2", applicantName: "Lerato Dlamini", status: "UnderReview", requestedAmount: 35000, requestedTermMonths: 18, purpose: "Business expansion", creditScore: 640, riskBand: "Medium", submittedAt: "2024-01-18T14:30:00Z" },
  { id: "a3", applicantId: "3", applicantName: "Sipho Nkosi", status: "Submitted", requestedAmount: 20000, requestedTermMonths: 12, purpose: "Vehicle repair", submittedAt: "2024-01-20T09:15:00Z" },
  { id: "a4", applicantId: "4", applicantName: "Nomsa Zulu", status: "Approved", requestedAmount: 100000, requestedTermMonths: 36, purpose: "Property investment", creditScore: 780, riskBand: "Low", submittedAt: "2024-01-10T08:00:00Z" },
  { id: "a5", applicantId: "5", applicantName: "Bongani Sithole", status: "Rejected", requestedAmount: 15000, requestedTermMonths: 12, purpose: "Debt consolidation", creditScore: 480, riskBand: "High", submittedAt: "2024-01-22T11:00:00Z" },
  { id: "a6", applicantId: "1", applicantName: "Thabo Mokoena", status: "Draft", requestedAmount: 25000, requestedTermMonths: 18, purpose: "Education fees" },
]

const mockLoans: Loan[] = [
  { id: "l1", applicationId: "a1", applicantId: "1", applicantName: "Thabo Mokoena", principal: 50000, interestRate: 0.18, termMonths: 24, status: "Active", outstandingBalance: 42000, monthlyInstallment: 2485.50, disbursementDate: "2024-01-20", createdAt: "2024-01-20T10:00:00Z" },
  { id: "l2", applicationId: "a4", applicantId: "4", applicantName: "Nomsa Zulu", principal: 100000, interestRate: 0.15, termMonths: 36, status: "Active", outstandingBalance: 92000, monthlyInstallment: 3466.20, disbursementDate: "2024-01-15", createdAt: "2024-01-15T08:00:00Z" },
  { id: "l3", applicationId: "a2", applicantId: "2", applicantName: "Lerato Dlamini", principal: 35000, interestRate: 0.22, termMonths: 18, status: "PendingDisbursement", outstandingBalance: 35000, monthlyInstallment: 2150.80, createdAt: "2024-01-25T14:30:00Z" },
  { id: "l4", applicationId: "a3", applicantId: "3", applicantName: "Sipho Nkosi", principal: 20000, interestRate: 0.20, termMonths: 12, status: "PendingDisbursement", outstandingBalance: 20000, monthlyInstallment: 1850.40, createdAt: "2024-01-28T09:15:00Z" },
  { id: "l5", applicationId: "a5", applicantId: "5", applicantName: "Bongani Sithole", principal: 15000, interestRate: 0.28, termMonths: 12, status: "Settled", outstandingBalance: 0, monthlyInstallment: 1450.20, disbursementDate: "2023-06-15", createdAt: "2023-06-15T11:00:00Z" },
]

const mockRepayments: Repayment[] = [
  { id: "r1", loanId: "l1", installmentNumber: 1, amountDue: 2485.50, amountPaid: 2485.50, status: "Paid", dueDate: "2024-02-20", paidDate: "2024-02-18", paymentReference: "PAY001" },
  { id: "r2", loanId: "l1", installmentNumber: 2, amountDue: 2485.50, amountPaid: 2485.50, status: "Paid", dueDate: "2024-03-20", paidDate: "2024-03-19", paymentReference: "PAY002" },
  { id: "r3", loanId: "l1", installmentNumber: 3, amountDue: 2485.50, amountPaid: 2485.50, status: "Paid", dueDate: "2024-04-20", paidDate: "2024-04-21", paymentReference: "PAY003" },
  { id: "r4", loanId: "l1", installmentNumber: 4, amountDue: 2485.50, amountPaid: 2485.50, status: "Paid", dueDate: "2024-05-20", paidDate: "2024-05-18", paymentReference: "PAY004" },
  { id: "r5", loanId: "l1", installmentNumber: 5, amountDue: 2485.50, amountPaid: 0, status: "Scheduled", dueDate: "2024-06-20" },
  { id: "r6", loanId: "l1", installmentNumber: 6, amountDue: 2485.50, amountPaid: 0, status: "Scheduled", dueDate: "2024-07-20" },
  { id: "r7", loanId: "l2", installmentNumber: 1, amountDue: 3466.20, amountPaid: 3466.20, status: "Paid", dueDate: "2024-02-15", paidDate: "2024-02-14", paymentReference: "PAY010" },
  { id: "r8", loanId: "l2", installmentNumber: 2, amountDue: 3466.20, amountPaid: 3466.20, status: "Paid", dueDate: "2024-03-15", paidDate: "2024-03-15", paymentReference: "PAY011" },
  { id: "r9", loanId: "l2", installmentNumber: 3, amountDue: 3466.20, amountPaid: 0, status: "Scheduled", dueDate: "2024-04-15" },
]

const mockStats: DashboardStats = {
  totalLoans: 5,
  totalOutstanding: 134000,
  totalDisbursed: 205000,
  activeApplications: 6,
  approvalRate: 68,
  avgLoanAmount: 41000,
}

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

export function useDashboardStats() {
  const [data, setData] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(600).then(() => {
      setData(mockStats)
      setLoading(false)
    })
  }, [])

  return { data, loading }
}

export function useLoans(status?: string) {
  const [data, setData] = useState<Loan[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(500).then(() => {
      let filtered = mockLoans
      if (status && status !== "all") {
        filtered = mockLoans.filter((l) => l.status.toLowerCase() === status.toLowerCase())
      }
      setData(filtered)
      setLoading(false)
    })
  }, [status])

  return { data, loading }
}

export function useLoan(id: string) {
  const [data, setData] = useState<Loan | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(400).then(() => {
      const loan = mockLoans.find((l) => l.id === id) || null
      setData(loan)
      setLoading(false)
    })
  }, [id])

  return { data, loading }
}

export function useLoanRepayments(loanId: string) {
  const [data, setData] = useState<Repayment[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(400).then(() => {
      setData(mockRepayments.filter((r) => r.loanId === loanId))
      setLoading(false)
    })
  }, [loanId])

  return { data, loading }
}

export function useApplications(status?: string) {
  const [data, setData] = useState<LoanApplication[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(500).then(() => {
      let filtered = mockApplications
      if (status && status !== "all") {
        filtered = mockApplications.filter((a) => a.status === status)
      }
      setData(filtered)
      setLoading(false)
    })
  }, [status])

  return { data, loading }
}

export function useApplicants() {
  const [data, setData] = useState<Applicant[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(500).then(() => {
      setData(mockApplicants)
      setLoading(false)
    })
  }, [])

  return { data, loading }
}

export function useApplicant(id: string) {
  const [data, setData] = useState<Applicant | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    delay(400).then(() => {
      const applicant = mockApplicants.find((a) => a.id === id) || null
      setData(applicant)
      setLoading(false)
    })
  }, [id])

  return { data, loading }
}
