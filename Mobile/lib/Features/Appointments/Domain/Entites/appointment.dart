/// Same order as the BookingStatus enum in the backend, which sends it as a number.
enum AppointmentStatus {
  pending,
  confirmed,
  cancelled,
  completed,
  rejected,
  expired,
}

enum AppointmentFilter { upcoming, past, cancelled }

class Appointment {
  final String id;
  final String providerId;
  final DateTime date;
  final AppointmentStatus status;
  final String? notes;

  /// The booking only has the provider id, so these are filled from the
  /// providers search and stay empty if the provider is not found there.
  final String providerName;
  final String address;
  final String phoneNumber;

  const Appointment({
    required this.id,
    required this.providerId,
    required this.date,
    required this.status,
    this.notes,
    this.providerName = '',
    this.address = '',
    this.phoneNumber = '',
  });

  Appointment withProvider({
    required String providerName,
    required String address,
    required String phoneNumber,
  }) {
    return Appointment(
      id: id,
      providerId: providerId,
      date: date,
      status: status,
      notes: notes,
      providerName: providerName,
      address: address,
      phoneNumber: phoneNumber,
    );
  }

  /// Pending or confirmed, and the time has not passed yet.
  bool get isUpcoming => filter == AppointmentFilter.upcoming;

  AppointmentFilter get filter {
    switch (status) {
      case AppointmentStatus.cancelled:
      case AppointmentStatus.rejected:
      case AppointmentStatus.expired:
        return AppointmentFilter.cancelled;
      case AppointmentStatus.completed:
        return AppointmentFilter.past;
      case AppointmentStatus.pending:
      case AppointmentStatus.confirmed:
        return date.isAfter(DateTime.now())
            ? AppointmentFilter.upcoming
            : AppointmentFilter.past;
    }
  }

  String get title => providerName.isEmpty ? 'Provider' : providerName;

  static const _months = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];

  String get dayLabel {
    const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    return '${days[date.weekday - 1]}, ${date.day} ${_months[date.month - 1]}';
  }

  String get timeLabel {
    final hour = date.hour % 12 == 0 ? 12 : date.hour % 12;
    final minute = date.minute.toString().padLeft(2, '0');

    return '$hour:$minute ${date.hour < 12 ? 'AM' : 'PM'}';
  }

  /// Like "Thursday, Oct 24 • 02:30 PM", with the year only if it is not this one.
  String get dateLabel {
    const days = [
      'Monday',
      'Tuesday',
      'Wednesday',
      'Thursday',
      'Friday',
      'Saturday',
      'Sunday',
    ];
    final day = '${date.day}'.padLeft(2, '0');
    final month = _months[date.month - 1];
    final year = date.year == DateTime.now().year ? '' : ' ${date.year}';

    return '${days[date.weekday - 1]}, $month $day$year • '
        '${timeLabel.padLeft(8, '0')}';
  }
}
