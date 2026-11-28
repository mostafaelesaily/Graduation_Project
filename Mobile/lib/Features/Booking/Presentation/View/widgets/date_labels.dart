const weekdayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

const monthNames = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

/// Like "03:15 PM".
String timeLabel(DateTime time) {
  final hour = time.hour % 12 == 0 ? 12 : time.hour % 12;
  String two(int value) => '$value'.padLeft(2, '0');

  return '${two(hour)}:${two(time.minute)} ${time.hour < 12 ? 'AM' : 'PM'}';
}

/// Like "Oct 24, 2026 • 03:15 PM".
String dateTimeLabel(DateTime date) {
  final month = monthNames[date.month - 1].substring(0, 3);

  return '$month ${date.day}, ${date.year} • ${timeLabel(date)}';
}
