enum SlotsStatus { loading, success, failure }

enum BookingStatus { initial, loading, success, failure }

class TimeSlot {
  final DateTime time;
  final bool isAvailable;

  const TimeSlot({required this.time, required this.isAvailable});
}

class BookingState {
  final DateTime selectedDay;
  final SlotsStatus slotsStatus;
  final List<TimeSlot> slots;
  final DateTime? selectedSlot;
  final BookingStatus status;

  /// The error on failure, or a warning to show after a successful booking.
  final String? message;

  const BookingState({
    required this.selectedDay,
    this.slotsStatus = SlotsStatus.loading,
    this.slots = const [],
    this.selectedSlot,
    this.status = BookingStatus.initial,
    this.message,
  });

  BookingState copyWith({
    DateTime? selectedDay,
    SlotsStatus? slotsStatus,
    List<TimeSlot>? slots,
    DateTime? selectedSlot,
    bool clearSlot = false,
    BookingStatus? status,
    String? message,
  }) {
    return BookingState(
      selectedDay: selectedDay ?? this.selectedDay,
      slotsStatus: slotsStatus ?? this.slotsStatus,
      slots: slots ?? this.slots,
      selectedSlot: clearSlot ? null : selectedSlot ?? this.selectedSlot,
      status: status ?? this.status,
      message: message,
    );
  }
}
