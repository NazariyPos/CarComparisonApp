import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <section className="page-card">
      <h2>Page not found</h2>
      <p>This route does not exist in CarComparisonClient.</p>
      <p>
        Go back to <Link to="/">Home page</Link>.
      </p>
    </section>
  )
}
