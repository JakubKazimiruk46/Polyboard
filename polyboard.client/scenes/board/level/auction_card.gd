extends CanvasLayer

var total_time_in_secs : int = 10

func on_auction_time_timeout():
	total_time_in_secs-=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$VBoxContainer/AuctionTimeLeft.text = '%02d:%02d' % [m, s]
	if total_time_in_secs == 0:
		total_time_in_secs = 10
		$AuctionTime.stop()
		self.visible = false
		$VBoxContainer/AuctionTimeLeft.text = ''
	
