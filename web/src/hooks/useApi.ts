import { useState, useEffect } from "react"
import { api } from "@/lib/api"
import type {
  Applicant,
  Loan,
  LoanApplication,
  Repayment,
} from "@/types"

export function useLoans(status?: string) {
  const [data, setData] = useState<Loan[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setLoading(true)
    api.loans
      .list(status === "all" ? undefined : status)
      .then((res) => setData(res.Items))
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [status])

  return { data, loading, error }
}

export function useLoan(id: string) {
  const [data, setData] = useState<Loan | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    api.loans
      .get(id)
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [id])

  return { data, loading, error }
}

export function useLoanRepayments(loanId: string) {
  const [data, setData] = useState<Repayment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!loanId) return
    setLoading(true)
    api.loans
      .repayments(loanId)
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [loanId])

  return { data, loading, error }
}

export function useApplications(status?: string) {
  const [data, setData] = useState<LoanApplication[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setLoading(true)
    api.applications
      .list(status === "all" ? undefined : status)
      .then((res) => setData(res.Items))
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [status])

  return { data, loading, error }
}

export function useApplicants() {
  const [data, setData] = useState<Applicant[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setLoading(true)
    api.applicants
      .list()
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  return { data, loading, error }
}

export function useApplicant(id: string) {
  const [data, setData] = useState<Applicant | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    api.applicants
      .get(id)
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [id])

  return { data, loading, error }
}

export function useDashboardStats() {
  const [data, setData] = useState<{
    TotalLoans: number
    TotalOutstanding: number
    TotalDisbursed: number
    ActiveApplications: number
    ApprovalRate: number
    AvgLoanAmount: number
  } | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([
      api.loans.list(),
      api.applications.list(),
    ])
      .then(([loansRes, appsRes]) => {
        const loans = loansRes.Items
        const apps = appsRes.Items

        const totalDisbursed = loans.reduce((s, l) => s + l.PrincipalAmount, 0)
        const totalOutstanding = loans.reduce((s, l) => s + l.OutstandingBalance, 0)
        const avgLoanAmount = loans.length > 0 ? totalDisbursed / loans.length : 0

        const approvedApps = apps.filter(
          (a) => a.Status === "Approved"
        ).length
        const approvalRate =
          apps.length > 0 ? Math.round((approvedApps / apps.length) * 100) : 0

        setData({
          TotalLoans: loans.length,
          TotalOutstanding: totalOutstanding,
          TotalDisbursed: totalDisbursed,
          ActiveApplications: apps.length,
          ApprovalRate: approvalRate,
          AvgLoanAmount: Math.round(avgLoanAmount),
        })
      })
      .catch(() => setData(null))
      .finally(() => setLoading(false))
  }, [])

  return { data, loading }
}
