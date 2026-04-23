import { useParams, Link } from "react-router-dom"
import {
  ArrowLeft,
  Banknote,
  Calendar,
  Clock,
  User,
  CheckCircle2,
  Circle,
} from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { useLoan, useLoanRepayments } from "@/hooks/useApi"

function formatZAR(amount: number) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency: "ZAR",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

function StatusBadge({ status }: { status: string }) {
  const variantMap: Record<string, "success" | "warning" | "destructive" | "secondary"> = {
    Active: "success",
    PendingDisbursement: "warning",
    Settled: "secondary",
    Defaulted: "destructive",
  }
  return <Badge variant={variantMap[status] || "secondary"}>{status}</Badge>
}

export default function LoanDetail() {
  const { id } = useParams<{ id: string }>()
  const { data: loan, loading: loanLoading } = useLoan(id || "")
  const { data: repayments, loading: repaymentsLoading } = useLoanRepayments(id || "")

  if (loanLoading) {
    return (
      <div className="space-y-6">
        <div className="h-8 w-48 bg-muted animate-pulse rounded" />
        <div className="grid gap-4 lg:grid-cols-3">
          <div className="h-32 bg-muted animate-pulse rounded-xl" />
          <div className="h-32 bg-muted animate-pulse rounded-xl" />
          <div className="h-32 bg-muted animate-pulse rounded-xl" />
        </div>
      </div>
    )
  }

  if (!loan) {
    return (
      <div className="flex flex-col items-center justify-center py-24">
        <Banknote className="h-12 w-12 text-muted-foreground/30 mb-4" />
        <h2 className="text-xl font-semibold">Loan not found</h2>
        <p className="text-muted-foreground mt-1">The loan you're looking for doesn't exist</p>
        <Button variant="outline" className="mt-6">
          <Link to="/loans" className="flex items-center">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Loans
          </Link>
        </Button>
      </div>
    )
  }

  const progressPercent = loan.principal > 0
    ? ((loan.principal - loan.outstandingBalance) / loan.principal) * 100
    : 0

  const paidRepayments = repayments.filter((r) => r.status === "Paid").length
  const totalRepayments = repayments.length

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Link to="/loans" className="hover:text-foreground transition-colors">Loans</Link>
        <span>/</span>
        <span className="text-foreground font-medium">{loan.id.slice(0, 8)}</span>
      </div>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Loan Details</h1>
          <div className="flex items-center gap-3 mt-2">
            <StatusBadge status={loan.status} />
            <span className="text-sm text-muted-foreground">ID: {loan.id}</span>
          </div>
        </div>
        <Button variant="outline" size="sm">
          <Link to="/loans" className="flex items-center">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Link>
        </Button>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="border-border/50">
          <CardHeader className="pb-3">
            <CardDescription>Principal Amount</CardDescription>
            <CardTitle className="text-3xl">{formatZAR(loan.principal)}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Banknote className="h-4 w-4" />
              <span>Rate: {(loan.interestRate * 100).toFixed(1)}% | Term: {loan.termMonths} months</span>
            </div>
          </CardContent>
        </Card>

        <Card className="border-border/50">
          <CardHeader className="pb-3">
            <CardDescription>Outstanding Balance</CardDescription>
            <CardTitle className="text-3xl">{formatZAR(loan.outstandingBalance)}</CardTitle>
          </CardHeader>
          <CardContent>
            <Progress value={progressPercent} max={100} variant="success" className="h-2" />
            <p className="text-xs text-muted-foreground mt-2">
              {progressPercent.toFixed(1)}% repaid
            </p>
          </CardContent>
        </Card>

        <Card className="border-border/50">
          <CardHeader className="pb-3">
            <CardDescription>Monthly Installment</CardDescription>
            <CardTitle className="text-3xl">{formatZAR(loan.monthlyInstallment)}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Calendar className="h-4 w-4" />
              <span>Disbursed: {loan.disbursementDate || "Pending"}</span>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Applicant Info */}
      <Card className="border-border/50">
        <CardHeader>
          <CardTitle>Applicant Information</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4">
            <div className="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center text-primary text-lg font-semibold">
              <User className="h-6 w-6" />
            </div>
            <div>
              <p className="text-lg font-semibold">{loan.applicantName}</p>
              <p className="text-sm text-muted-foreground">Applicant ID: {loan.applicantId}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Repayment Schedule */}
      <Card className="border-border/50">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Repayment Schedule</CardTitle>
              <CardDescription>
                {paidRepayments} of {totalRepayments} installments paid
              </CardDescription>
            </div>
            <div className="flex items-center gap-4 text-sm">
              <div className="flex items-center gap-1.5">
                <CheckCircle2 className="h-4 w-4 text-emerald-500" />
                <span className="text-muted-foreground">Paid</span>
              </div>
              <div className="flex items-center gap-1.5">
                <Circle className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Scheduled</span>
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {repaymentsLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="h-14 bg-muted animate-pulse rounded-lg" />
              ))}
            </div>
          ) : repayments.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <Clock className="mx-auto h-10 w-10 mb-3 opacity-30" />
              No repayment schedule available
            </div>
          ) : (
            <div className="space-y-2">
              {repayments.map((repayment) => (
                <div
                  key={repayment.id}
                  className="flex items-center justify-between rounded-lg border border-border/50 p-4 transition-colors hover:bg-muted/20"
                >
                  <div className="flex items-center gap-4">
                    {repayment.status === "Paid" ? (
                      <CheckCircle2 className="h-5 w-5 text-emerald-500" />
                    ) : (
                      <Circle className="h-5 w-5 text-muted-foreground" />
                    )}
                    <div>
                      <p className="text-sm font-medium">
                        Installment {repayment.installmentNumber}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        Due: {repayment.dueDate}
                        {repayment.paidDate && ` • Paid: ${repayment.paidDate}`}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium">{formatZAR(repayment.amountDue)}</p>
                    {repayment.status === "Paid" && (
                      <p className="text-xs text-emerald-400">
                        Paid {formatZAR(repayment.amountPaid)}
                      </p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
