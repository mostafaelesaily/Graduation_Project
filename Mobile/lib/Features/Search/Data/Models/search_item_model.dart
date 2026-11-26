import '../../Domain/Entities/search_item_entity.dart';

class SearchItemModel extends SearchItemEntity {
  const SearchItemModel({
    required super.id,
    required super.name,
    required super.specialty,
    required super.rating,
    required super.reviewsCount,
    required super.distance,
    required super.isVerified,
    super.imageUrl,
  });

  factory SearchItemModel.fromJson(Map<String, dynamic> json) {
    return SearchItemModel(
      id: json['id'].toString(),
      name: json['name'] ?? '',
      specialty: json['specialty'] ?? '',
      rating: (json['rating'] as num).toDouble(),
      reviewsCount: json['reviewsCount'] ?? 0,
      distance: (json['distance'] as num).toDouble(),
      isVerified: json['isVerified'] ?? false,
      imageUrl: json['imageUrl'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'specialty': specialty,
      'rating': rating,
      'reviewsCount': reviewsCount,
      'distance': distance,
      'isVerified': isVerified,
      'imageUrl': imageUrl,
    };
  }
}
