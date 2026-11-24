import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_cubit.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_state.dart';
import 'package:medlink/Features/Search/Data/Models/search_item_model.dart';

class SearchTab extends StatelessWidget {
  final List<SearchItemModel> results;
  final Set<String> favoriteIds;

  final String searchQuery;
  final String selectedCategory;
  final String selectedSort;
  final bool verifiedOnly;
  final bool mapView;

  final ValueChanged<String>? onSearchChanged;
  final ValueChanged<String>? onCategoryChanged;
  final ValueChanged<String>? onSortChanged;
  final ValueChanged<bool>? onVerifiedChanged;
  final ValueChanged<bool>? onViewChanged;
  final ValueChanged<SearchItemModel>? onFavorite;
  final ValueChanged<SearchItemModel>? onResultTap;
  final ValueChanged<SearchItemModel>? onBook;

  const SearchTab({
    super.key,
    this.results = const [
      SearchItemModel(
        id: '0',
        name: 'name',
        specialty: 'specialty',
        rating: 2.0,
        reviewsCount: 2,
        distance: 1,
        isVerified: true,
      ),
      SearchItemModel(
        id: '1',
        name: 'name',
        specialty: 'specialty',
        rating: 2.0,
        reviewsCount: 2,
        distance: 1,
        isVerified: false,
      ),
    ],
    this.favoriteIds = const {},
    this.searchQuery = '',
    this.selectedCategory = 'Cardiology',
    this.selectedSort = 'Recommended',
    this.verifiedOnly = true,
    this.mapView = false,
    this.onSearchChanged,
    this.onCategoryChanged,
    this.onSortChanged,
    this.onVerifiedChanged,
    this.onViewChanged,
    this.onFavorite,
    this.onResultTap,
    this.onBook,
  });

  static const Color primary = Color(0xFF2980F7);
  static const Color background = Color(0xFFF6F9FD);
  static const Color dark = Color(0xFF153454);
  static const Color secondary = Color(0xFF7189A7);
  static const Color lightBlue = Color(0xFFE8F2FF);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: background,
      body: SafeArea(
        bottom: false,
        child: ListView(
          padding: EdgeInsets.only(bottom: 24.h),
          children: [
            _buildSearchBar(context),
            SizedBox(height: 14.h),
            _buildFilters(),
            SizedBox(height: 20.h),
            _buildResultsHeader(),
            SizedBox(height: 14.h),
            if (results.isEmpty)
              _buildEmptyState()
            else
              ...results.map(
                (item) => Padding(
                  padding: EdgeInsets.only(bottom: 14.h),
                  child: _ResultCard(
                    item: item,
                    isFavorite: favoriteIds.contains(item.id),
                    onFavorite: () => onFavorite?.call(item),
                    onTap: () => onResultTap?.call(item),
                    onBook: () => onBook?.call(item),
                  ),
                ),
              ),
            SizedBox(height: 4.h),
            _buildMapPreview(),
          ],
        ),
      ),
    );
  }

  // SEARCH BAR

  Widget _buildSearchBar(BuildContext context) {
    return Padding(
      padding: EdgeInsets.fromLTRB(16.w, 12.h, 16.w, 0),
      child: Container(
        height: 48.h,
        padding: EdgeInsets.symmetric(horizontal: 12.w),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(22.r),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.035),
              blurRadius: 8,
            ),
          ],
        ),
        child: Row(
          children: [
            IconButton(
              onPressed: () => context.read<HomeCubit>().changeHomeStatus(
                status: HomeStatus.home,
              ),
              icon: Icon(Icons.arrow_back, color: secondary, size: 23.sp),
              style: IconButton.styleFrom(
                padding: EdgeInsets.zero,
                visualDensity: .compact,
                tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                iconSize: 23.sp,
              ),
            ),
            SizedBox(width: 8.w),
            Expanded(
              child: TextFormField(
                key: ValueKey(searchQuery),
                initialValue: searchQuery,
                onChanged: onSearchChanged,
                style: TextStyle(fontSize: 14.sp, color: dark),
                decoration: const InputDecoration(
                  border: InputBorder.none,
                  hintText: 'Search doctors, specialties...',
                  isDense: true,
                  contentPadding: EdgeInsets.zero,
                ),
              ),
            ),
            if (searchQuery.isNotEmpty)
              InkWell(
                onTap: () => onSearchChanged?.call(''),
                child: Icon(Icons.close, color: secondary, size: 19.sp),
              ),
          ],
        ),
      ),
    );
  }

  // FILTERS

  Widget _buildFilters() {
    return Padding(
      padding: EdgeInsets.fromLTRB(16.w, 12.h, 16.w, 0),
      child: Row(
        mainAxisAlignment: .start,
        spacing: 10.w,
        children: [
          _filterChip(
            'Verified',
            selected: verifiedOnly,
            icon: Icons.verified_outlined,
            onTap: () => onVerifiedChanged?.call(!verifiedOnly),
          ),
          _filterChip('Specialty', icon: Icons.keyboard_arrow_down),
        ],
      ),
    );
  }

  Widget _filterChip(
    String label, {
    bool selected = false,
    IconData? icon,
    VoidCallback? onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(25.r),
      child: Container(
        padding: EdgeInsets.symmetric(horizontal: 14.w, vertical: 9.h),
        decoration: BoxDecoration(
          color: selected ? primary : Colors.white,
          borderRadius: BorderRadius.circular(25.r),
        ),
        child: Row(
          children: [
            if (icon != null) ...[
              Icon(icon, size: 14.sp, color: selected ? Colors.white : primary),
              SizedBox(width: 4.w),
            ],
            Text(
              label,
              style: TextStyle(
                color: selected ? Colors.white : dark,
                fontSize: 13.sp,
              ),
            ),
            if (selected) ...[
              SizedBox(width: 8.w),
              Icon(Icons.check, size: 15.sp, color: Colors.white),
            ],
          ],
        ),
      ),
    );
  }

  // RESULTS HEADER

  Widget _buildResultsHeader() {
    return Padding(
      padding: EdgeInsets.symmetric(horizontal: 18.w),
      child: Row(
        children: [
          Text(
            '${results.length} providers',
            style: TextStyle(
              color: dark,
              fontSize: 15.sp,
              fontWeight: FontWeight.bold,
            ),
          ),
          const Spacer(),
          PopupMenuButton<String>(
            onSelected: onSortChanged,
            initialValue: selectedSort,
            padding: EdgeInsets.zero,
            child: Row(
              children: [
                Text(
                  'Sort: $selectedSort',
                  style: TextStyle(
                    color: primary,
                    fontSize: 13.sp,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                Icon(Icons.unfold_more, color: primary, size: 19.sp),
              ],
            ),
            itemBuilder: (_) => const [
              PopupMenuItem(value: 'Recommended', child: Text('Recommended')),
              PopupMenuItem(value: 'Rating', child: Text('Rating')),
              PopupMenuItem(value: 'Distance', child: Text('Distance')),
            ],
          ),
        ],
      ),
    );
  }

  // EMPTY STATE

  Widget _buildEmptyState() {
    return Padding(
      padding: EdgeInsets.symmetric(horizontal: 25.w, vertical: 35.h),
      child: Column(
        children: [
          Icon(Icons.search_off_rounded, size: 55.sp, color: secondary),
          SizedBox(height: 12.h),
          Text(
            'No providers found',
            style: TextStyle(
              color: dark,
              fontSize: 17.sp,
              fontWeight: FontWeight.bold,
            ),
          ),
          SizedBox(height: 6.h),
          Text(
            'Try changing your search or filters.',
            textAlign: TextAlign.center,
            style: TextStyle(color: secondary, fontSize: 13.sp),
          ),
        ],
      ),
    );
  }

  // MAP PREVIEW

  Widget _buildMapPreview() {
    return Container(
      margin: EdgeInsets.symmetric(horizontal: 16.w),
      padding: EdgeInsets.all(12.w),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20.r),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Icon(Icons.explore_outlined, color: primary),
              SizedBox(width: 8.w),
              Text(
                'Map View Preview',
                style: TextStyle(
                  color: dark,
                  fontWeight: FontWeight.w600,
                  fontSize: 14.sp,
                ),
              ),
              const Spacer(),
              Text(
                '${results.length} Pins Loaded',
                style: TextStyle(color: primary, fontSize: 13.sp),
              ),
            ],
          ),
          SizedBox(height: 12.h),
          Container(
            height: 125.h,
            width: double.infinity,
            decoration: BoxDecoration(
              color: const Color(0xFFE4F0E8),
              borderRadius: BorderRadius.circular(12.r),
            ),
            child: Stack(
              alignment: Alignment.center,
              children: [
                Icon(
                  Icons.map_outlined,
                  size: 95.sp,
                  color: const Color(0xFF9CBCA9),
                ),
                Container(
                  padding: EdgeInsets.symmetric(
                    horizontal: 14.w,
                    vertical: 9.h,
                  ),
                  decoration: BoxDecoration(
                    color: primary,
                    borderRadius: BorderRadius.circular(25.r),
                  ),
                  child: Text(
                    '${results.length} Providers',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 14.sp,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _navItem(IconData icon, String label, {bool selected = false}) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, color: selected ? primary : secondary, size: 23.sp),
        SizedBox(height: 3.h),
        Text(
          label,
          style: TextStyle(
            color: selected ? primary : secondary,
            fontSize: 11.sp,
          ),
        ),
        SizedBox(height: 3.h),
        if (selected)
          Container(
            width: 5.w,
            height: 5.w,
            decoration: const BoxDecoration(
              color: primary,
              shape: BoxShape.circle,
            ),
          ),
      ],
    );
  }
}

// =====================================================
// PROVIDER CARD
// =====================================================

class _ResultCard extends StatelessWidget {
  final SearchItemModel item;
  final bool isFavorite;

  final VoidCallback onFavorite;
  final VoidCallback onTap;
  final VoidCallback onBook;

  const _ResultCard({
    required this.item,
    required this.isFavorite,
    required this.onFavorite,
    required this.onTap,
    required this.onBook,
  });

  static const Color primary = SearchTab.primary;
  static const Color dark = SearchTab.dark;
  static const Color secondary = SearchTab.secondary;
  static const Color lightBlue = SearchTab.lightBlue;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: EdgeInsets.symmetric(horizontal: 16.w),
      padding: EdgeInsets.all(13.w),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20.r),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.025),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildImage(),
              SizedBox(width: 12.w),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                        color: dark,
                        fontWeight: FontWeight.w500,
                        fontSize: 14.sp,
                      ),
                    ),
                    SizedBox(height: 4.h),
                    Text(
                      item.specialty,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(color: secondary, fontSize: 12.5.sp),
                    ),
                    SizedBox(height: 5.h),
                    _buildRating(),
                  ],
                ),
              ),
              InkWell(
                onTap: onFavorite,
                child: Icon(
                  isFavorite ? Icons.favorite : Icons.favorite_border,
                  color: isFavorite ? Colors.red : secondary,
                  size: 22.sp,
                ),
              ),
            ],
          ),

          if (item.isVerified) ...[
            SizedBox(height: 12.h),
            _tag(Icons.verified_outlined, 'Verified'),
          ],

          SizedBox(height: 13.h),

          Row(
            children: [
              Expanded(child: _actionButton('View Profile', false, onTap)),
              SizedBox(width: 8.w),
              Expanded(child: _actionButton('Book Visit', true, onBook)),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildImage() {
    return Container(
      width: 55.w,
      height: 55.w,
      decoration: BoxDecoration(
        color: lightBlue,
        borderRadius: BorderRadius.circular(15.r),
      ),
      clipBehavior: Clip.antiAlias,
      child: item.imageUrl != null && item.imageUrl!.isNotEmpty
          ? Image.network(
              item.imageUrl!,
              fit: BoxFit.cover,
              errorBuilder: (_, _, _) => _imagePlaceholder(),
            )
          : _imagePlaceholder(),
    );
  }

  Widget _imagePlaceholder() {
    return Icon(Icons.person, color: primary, size: 31.sp);
  }

  Widget _buildRating() {
    return Wrap(
      crossAxisAlignment: WrapCrossAlignment.center,
      spacing: 4.w,
      children: [
        Icon(Icons.star, color: primary, size: 16.sp),
        Text(
          item.rating.toStringAsFixed(1),
          style: TextStyle(
            color: primary,
            fontWeight: FontWeight.w600,
            fontSize: 12.sp,
          ),
        ),
        Text('•', style: TextStyle(color: secondary)),
        Text(
          '(${item.reviewsCount} reviews)',
          style: TextStyle(color: secondary, fontSize: 11.5.sp),
        ),
        Text('•', style: TextStyle(color: secondary)),
        Text(
          '${item.distance.toStringAsFixed(1)} mi',
          style: TextStyle(color: secondary, fontSize: 11.5.sp),
        ),
      ],
    );
  }

  Widget _tag(IconData icon, String text) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 9.w, vertical: 7.h),
      decoration: BoxDecoration(
        color: lightBlue,
        borderRadius: BorderRadius.circular(20.r),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 13.sp, color: primary),
          SizedBox(width: 4.w),
          Text(
            text,
            style: TextStyle(
              color: primary,
              fontSize: 11.5.sp,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _actionButton(String label, bool filled, VoidCallback onPressed) {
    return SizedBox(
      height: 39.h,
      child: ElevatedButton(
        onPressed: onPressed,
        style: ElevatedButton.styleFrom(
          elevation: 0,
          backgroundColor: filled ? primary : SearchTab.background,
          foregroundColor: filled ? Colors.white : dark,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12.r),
          ),
        ),
        child: FittedBox(
          fit: BoxFit.scaleDown,
          child: Text(
            label,
            style: TextStyle(fontSize: 12.5.sp, fontWeight: FontWeight.w600),
          ),
        ),
      ),
    );
  }
}
