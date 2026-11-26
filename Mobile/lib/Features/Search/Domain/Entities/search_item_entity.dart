class SearchItemEntity {
  final String id;
  final String name;
  final String specialty;
  final double rating;
  final int reviewsCount;
  final double distance;
  final bool isVerified;
  final String? imageUrl;

  const SearchItemEntity({
    required this.id,
    required this.name,
    required this.specialty,
    required this.rating,
    required this.reviewsCount,
    required this.distance,
    required this.isVerified,
    this.imageUrl,
  });
}
