export const BookingStatus = {
  Pending: 0,
  Confirmed: 1,
  Completed: 2,
  Cancelled: 3,
  NoShow: 4,
};

export function bookingStatusLabel(status) {
  switch (status) {
    case BookingStatus.Pending:
    case 'Pending':
      return 'Pending';
    case BookingStatus.Confirmed:
    case 'Confirmed':
      return 'Confirmed';
    case BookingStatus.Completed:
    case 'Completed':
      return 'Completed';
    case BookingStatus.Cancelled:
    case 'Cancelled':
      return 'Cancelled';
    case BookingStatus.NoShow:
    case 'NoShow':
      return 'No Show';
    default:
      return String(status ?? 'Unknown');
  }
}